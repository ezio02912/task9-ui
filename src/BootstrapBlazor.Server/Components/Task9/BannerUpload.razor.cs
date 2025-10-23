
namespace BootstrapBlazor.Server.Components.Task9;
public partial class BannerUpload : IDisposable
{
    #region Inject
  
    [Inject] private IFileManagerService FileManagerService { get; set; }
    [Inject, NotNull] private HttpClient? HttpClient { get; set; }
    [Inject, NotNull] private ToastService? Toast { get; set; }
    #endregion
    private CancellationTokenSource? TokenSource { get; set; }
    private List<FileInfoDto> Items = new();
    private List<FileInfoDto> ItemsSearch => Items.Where(x => x.Name.ToLower().Contains(SearchText.ToLower())).ToList();
    private List<TreeViewItem<FolderNode>>? FolderTrees { get; set; }
    private List<FolderNode> Folders = new();
    private string SelectedFolder = string.Empty;
    private string SearchText = string.Empty;
    private bool IsLoading{get;set;} = false;
    private DropUpload DropUpload { get; set; }



    protected override async Task OnInitializedAsync()
    {  
        base.OnInitialized();
        SelectedFolder = string.Empty; // Reset selected folder
        await LoadFolders(string.Empty); // Load all root folders
    }

    private async Task LoadFolders(string folderName)
    {
        try
        {
            var response = await FileManagerService.ListFoldersAsync(string.IsNullOrEmpty((folderName))? "All" : folderName);
            if (response.IsSuccess)
            {
                if (string.IsNullOrEmpty(SelectedFolder) || SelectedFolder == "All")
                {
                    // Load tất cả folders lần đầu
                    Folders = BuildFolderTree(response.Data);
                    SelectedFolder = "All";
                }
                else
                {
                    // Load folder con cho folder được chọn
                    // Normalize folderName to have trailing slash for comparison
                    var normalizedFolderName = folderName.EndsWith("/") ? folderName : folderName + "/";
                    var targetNode = FindFolderNodeByFullPath(Folders, normalizedFolderName);
                    if (targetNode != null)
                    {
                        // Clear children cũ và thêm children mới
                        targetNode.Children.Clear();
                        
                        // Tìm folder con trực tiếp - có thêm đúng 1 dấu / so với parent
                        var parentLevel = normalizedFolderName.Count(c => c == '/');
                        var childFolders = response.Data.Where(path => 
                            path.StartsWith(normalizedFolderName) && 
                            path.EndsWith("/") &&
                            path != normalizedFolderName &&
                            path.Count(c => c == '/') == parentLevel + 1).ToList();
                        
                        foreach (var childPath in childFolders)
                        {
                            var childName = childPath.TrimEnd('/').Split('/').Last();
                            var childNode = new FolderNode
                            {
                                Name = childName,
                                FullPath = childPath, // Giữ trailing slash để gọi API đầy đủ
                                ParentId = targetNode.Id,
                                Expanded = false
                            };
                            targetNode.Children.Add(childNode);
                        }
                        
                        // Set expanded nếu có children
                        targetNode.Expanded = targetNode.Children.Count > 0;
                    }
                }
                
                // Rebuild tree để đồng bộ TreeViewItem.Items với FolderNode.Children
                FolderTrees = CascadingTree(Folders);
                await LoadFileInFolder(SelectedFolder);
                
                // Force UI update
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            await Toast.Error("Error", $"Lỗi khi tải danh sách thư mục: {ex.Message}");
        }
    }

    private async Task LoadFileInFolder(string selectedFolder)
    {
        try
        {
            IsLoading = true;
            StateHasChanged();


            var response = await FileManagerService.ListFileInFolderAsync(selectedFolder);
            if (response.IsSuccess)
            {
                Items = new  List<FileInfoDto>();
                // Normalize selectedFolder for consistent comparison
                var normalizedSelectedFolder = selectedFolder.EndsWith("/") ? selectedFolder : selectedFolder + "/";
                int indexFolder = normalizedSelectedFolder.TrimEnd('/').Split("/").Length;
                var folderNode = FindFolderNodeByFullPath(Folders, normalizedSelectedFolder);
                if(folderNode != null)  folderNode.Children = new List<FolderNode>();
                bool needUpdateTree = false;
                // gán file 
                foreach(var file in response.Data)
                {
                    int indexFile = file.Name.Split("/").Length;
                    if (indexFile > indexFolder + 2) continue;
                    if(file.Name.Split("/").Length == indexFolder + 1)
                    {
                        // chỉ lấy file ảnh các file khác ko cần add
                        if(file.Name.EndsWith(".jpg") || file.Name.EndsWith(".png") || file.Name.EndsWith(".jpeg") || file.Name.EndsWith(".webp")
                        || file.Name.EndsWith(".gif") || file.Name.EndsWith(".bmp") || file.Name.EndsWith(".tiff") || file.Name.EndsWith(".ico")
                        || file.Name.EndsWith(".svg") || file.Name.EndsWith(".heic") || file.Name.EndsWith(".heif") || file.Name.EndsWith(".heif")
                        || file.Name.EndsWith(".heif") || file.Name.EndsWith(".heif") || file.Name.EndsWith(".heif") || file.Name.EndsWith(".heif")
                        || file.Name.EndsWith(".heif") || file.Name.EndsWith(".heif") || file.Name.EndsWith(".heif") || file.Name.EndsWith(".heif")
                        )
                        {
                            Items.Add(file);
                        }
                    }else
                    {
                        needUpdateTree = true;
                        var folderName = file.Name.Split("/")[indexFolder];
                        var folderChild = new FolderNode
                        { 
                            Name = folderName, 
                            FullPath = normalizedSelectedFolder + folderName + "/", // Giữ trailing slash
                            ParentId = folderNode?.Id
                        };
                        if(folderNode.Children != null && folderNode.Children.Count(x => x.Name == folderName) == 0)
                        {
                            folderNode.Children.Add(folderChild);
                        }
                    }
                }

                if(needUpdateTree)
                {
                    FolderTrees = CascadingTree(Folders);
                }
            }
        }
        catch (Exception ex)
        {
            await Toast.Error("Error", $"Lỗi khi tải danh sách ảnh: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    
    private async Task OnSearchImageValueChanged(string v)
    {
        SearchText = v;
        StateHasChanged();
    }

     private async Task<QueryData<FileInfoDto>> OnQueryAsync(QueryPageOptions options)
    {
        await Task.Delay(200);
        var itemFilter = Items.Where(x => 
        string.IsNullOrEmpty(options.SearchText) || 
        x.Name.ToLower().Contains(options.SearchText.ToLower())).ToList();

        return new QueryData<FileInfoDto>()
        {
            Items = itemFilter,
            TotalCount = Items.Count
        };
    }

    private  async Task OnTreeItemClick(TreeViewItem<FolderNode> item)
    {
        // Set folder được chọn
        SelectedFolder = item.Value.FullPath;
        
        // Debug: Log current children count before loading
        System.Console.WriteLine($"Before load - Folder: {item.Value.Name}, Children count: {item.Value.Children.Count}");
        
        // Load folder con và files trong folder được chọn
        await LoadFolders(item.Value.FullPath);
        
        // Debug: Log children count after loading
        System.Console.WriteLine($"After load - Folder: {item.Value.Name}, Children count: {item.Value.Children.Count}");
        if (FolderTrees != null)
        {
            var treeItem = FindTreeViewItem(FolderTrees, item.Value.FullPath);
            if (treeItem != null)
            {
                System.Console.WriteLine($"TreeViewItem - Items count: {treeItem.Items?.Count ?? 0}, HasChildren: {treeItem.HasChildren}, IsExpand: {treeItem.IsExpand}");
            }
        }
        
        StateHasChanged();
    }
    
    private TreeViewItem<FolderNode>? FindTreeViewItem(List<TreeViewItem<FolderNode>> items, string fullPath)
    {
        foreach (var item in items)
        {
            if (item.Value.FullPath == fullPath)
                return item;
            
            if (item.Items?.Any() == true)
            {
                var found = FindTreeViewItem(item.Items.ToList(), fullPath);
                if (found != null)
                    return found;
            }
        }
        return null;
    }

    private async Task OnUpload()
    {
        try 
        {
            List<UploadFile> files = DropUpload.UploadFiles;
            if(files == null || files.Count == 0 )
            {
                await Toast.Error("Lỗi", "Không có file để upload");
                return;
            }

            foreach(var file in files)
            {
                var response = await FileManagerService.UploadFileAsync(SelectedFolder, file.File);
                if(response.IsSuccess)
                {
                    await Toast.Success($"Tải lên file {file.File.Name} thành công");
                }
                else
                {       
                    await Toast.Error("Lỗi", $"Tải lên file {file.File.Name} thất bại");
                }
            }
            await LoadFileInFolder(SelectedFolder);
            DropUpload.Reset();
        }
        catch (Exception ex)
        {
            await Toast.Error("Lỗi", $"Lỗi khi upload file: {ex.Message}");
        }
    }


    private List<FolderNode> BuildFolderTree(List<string> folderPaths)
    {
        var root = new List<FolderNode>();

        // Build only root level folders initially - folders with only 1 slash at the end
        var rootFolders = folderPaths.Where(path => 
            path.EndsWith("/") && 
            path.Count(c => c == '/') == 1).ToList();
        
        foreach (var folderPath in rootFolders)
        {
            if (string.IsNullOrWhiteSpace(folderPath)) continue;
            
            // Remove trailing slash to get folder name
            var folderName = folderPath.TrimEnd('/');
            
            var newNode = new FolderNode 
            { 
                Name = folderName, 
                FullPath = folderPath, // Store without trailing slash for consistency
                ParentId = null,
                Expanded = false
            };
            
            root.Add(newNode);
        }
        
        return root;
    }

    private void AddFolderToTree(string fullPath)
    {
        var parts = fullPath.Trim('/').Split('/');
        List<FolderNode> currentLevel = Folders;
        FolderNode? parentNode = null;

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];
            if (string.IsNullOrWhiteSpace(part)) continue;
            var existing = currentLevel.FirstOrDefault(x => x.Name == part);
            if (existing == null)
            {
                var newNode = new FolderNode 
                { 
                    Name = part, 
                    FullPath = parts.Take(i + 1).Aggregate((a, b) => $"{a}/{b}") + "/", // Giữ trailing slash
                    ParentId = parentNode?.Id
                };
                currentLevel.Add(newNode);
                parentNode = newNode;
                currentLevel = newNode.Children;
            }
            else
            {
                parentNode = existing;
                currentLevel = existing.Children;
            }
        }
    }

    private FolderNode? FindFolderNodeByFullPath(List<FolderNode> nodes, string fullPath)
    {
        foreach (var node in nodes)
        {
            if (node.FullPath == fullPath)
                return node;
            if (node.Children.Count > 0)
            {
                var found = FindFolderNodeByFullPath(node.Children, fullPath);
                if (found != null)
                    return found;
            }
        }
        return null;
    }

    private bool ShouldExpandThisNode(object objectNode)
    {
        var node = (FolderNode)objectNode;
        return node.Expanded;
    }

    private  List<TreeViewItem<FolderNode>> CascadingTree(List<FolderNode> items)
    {
        // Build tree manually để đảm bảo TreeViewItem.Items sync với FolderNode.Children
        var rootItems = items.Where(x => string.IsNullOrEmpty(x.ParentId)).ToList();
        var result = new List<TreeViewItem<FolderNode>>();
        
        foreach (var rootItem in rootItems)
        {
            var treeViewItem = CreateTreeViewItem(rootItem, items);
            result.Add(treeViewItem);
        }
        
        return result;
    }
    
    private TreeViewItem<FolderNode> CreateTreeViewItem(FolderNode node, List<FolderNode> allNodes)
    {
        var treeViewItem = new TreeViewItem<FolderNode>(node)
        {
            Text = node.Text,
            Icon = node.Icon,
            IsActive = node.FullPath == SelectedFolder
        };
        
        // Build children từ FolderNode.Children
        if (node.Children?.Any() == true)
        {
            treeViewItem.Items = new List<TreeViewItem<FolderNode>>();
            foreach (var child in node.Children)
            {
                var childTreeViewItem = CreateTreeViewItem(child, allNodes);
                childTreeViewItem.Parent = treeViewItem;
                treeViewItem.Items.Add(childTreeViewItem);
            }
        }
        
        // Set properties
        var hasActualChildren = treeViewItem.Items?.Any() == true;
        treeViewItem.HasChildren = hasActualChildren || !node.Expanded;
        treeViewItem.IsExpand = node.Expanded && hasActualChildren;
        
        System.Console.WriteLine($"TreeViewItem created - {node.Name}: HasChildren={treeViewItem.HasChildren}, IsExpand={treeViewItem.IsExpand}, ItemsCount={treeViewItem.Items?.Count ?? 0}");
        
        return treeViewItem;
    }


    protected virtual void Dispose(bool disposing)
    {
        if (disposing && TokenSource != null)
        {
            TokenSource.Cancel();
            TokenSource.Dispose();
        }
    }
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}


 public class FolderNode
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public List<FolderNode> Children { get; set; } = new();

    public string? Text { get {return Name; } }
    public string Icon { get; set; } = "fa-solid fa-folder";

    public bool IsActive { get { return Expanded; } }
    public bool Expanded { get; set; } = false;
}