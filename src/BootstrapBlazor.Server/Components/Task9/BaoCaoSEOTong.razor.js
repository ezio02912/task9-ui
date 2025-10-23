import '../../../_content/BootstrapBlazor.Chart/js/chart.umd.js'
import Data from '../../../_content/BootstrapBlazor/modules/data.js'

export function initStackedDemo(id, dataChart) {
    console.log('initStackedDemo called for:', id);
    
    // Dispose existing chart if any
    const existingChart = Data.get(id);
    if (existingChart) {
        console.log('Destroying existing chart for:', id);
        existingChart.destroy();
        Data.remove(id);
    }
    
    const ctx = document.getElementById(id);
    if (!ctx) {
        console.error('Canvas element not found:', id);
        return;
    }
    
    
    const data = {
        labels: dataChart.labels,
        datasets: dataChart.datasets
    };

    // Custom plugin to draw data labels
    const labelPlugin = {
        id: 'customDataLabels',
        afterDraw: function(chart) {
            const ctx = chart.ctx;
            ctx.save();
            
            // Set font and text alignment
            ctx.font = '12px Arial';
            ctx.textAlign = 'center';
            ctx.textBaseline = 'middle';
            
            chart.data.datasets.forEach(function(dataset, datasetIndex) {
                const meta = chart.getDatasetMeta(datasetIndex);
                
                if (meta.visible !== false) {
                    meta.data.forEach(function(element, index) {
                        // Use original data for label display if available, otherwise use regular data
                        const originalValue = dataset.originalData ? dataset.originalData[index] : dataset.data[index];
                        const displayValue = originalValue !== null && originalValue !== undefined ? originalValue : dataset.data[index];
                        
                        // Show all values including 0
                        if (displayValue !== null && displayValue !== undefined && displayValue > 0) {
                            // Get element center position for stacked bar
                            const x = element.x;
                            
                            const barTop = element.y;           // Top of this segment
                            const barBase = element.base;       // Base of this segment
                            const centerY = (barTop + barBase) / 2;  // Middle of segment
                            
                            const lightBackgrounds = [0,1,2,3,4,5,6,7,8]; 
                            const needsDarkText = lightBackgrounds.includes(datasetIndex);
                            
                            if (needsDarkText) {
                                ctx.fillStyle = '#000000';
                                ctx.strokeStyle = '#FFFFFF';
                            } else {
                                ctx.fillStyle = '#FFFFFF';
                                ctx.strokeStyle = '#000000';
                            }
                            ctx.lineWidth = 2;
                            
                            ctx.strokeText(String(displayValue), x, centerY);
                            ctx.fillText(String(displayValue), x, centerY);
                        }
                    });
                }
            });
            
            ctx.restore();
        }
    };

    const config = {
        type: 'bar',
        data: data,
        plugins: [labelPlugin],
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                title: {
                    display: true,
                    text: 'Biểu đồ từ khoá / số lần lên top'
                },
                legend: {
                    display: true,
                    position: 'top'
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const dataset = context.dataset;
                            const dataIndex = context.dataIndex;
                            const originalValue = dataset.originalData ? dataset.originalData[dataIndex] : context.parsed.y;
                            const displayValue = originalValue !== null && originalValue !== undefined ? originalValue : context.parsed.y;
                            return dataset.label + ': ' + displayValue;
                        }
                    }
                }
            },
            scales: {
                x: {
                    stacked: true,
                    title: {
                        display: true,
                        text: 'Từ khóa / Tên miền'
                    }
                },
                y: {
                    stacked: true,
                    title: {
                        display: true,
                        text: 'Số lần lên top'
                    },
                    beginAtZero: true
                }
            }
        }
    };

    const chart = new Chart(ctx, config);
    Data.set(id, chart);
    
    // Add window resize listener for this chart
    window.addEventListener('resize', () => {
        setTimeout(() => {
            if (chart && !chart.destroyed) {
                chart.resize();
            }
        }, 100);
    });
}


export function updatedStacked(id, data) {
    const chart = Data.get(id);
    if (chart) {
        chart.data.datasets = data.datasets;
        chart.data.labels = data.labels;
        chart.update('active'); // Update with animation to trigger onComplete callback
    } else {
        console.warn('Chart not found for update:', id);
    }
}

export function checkChartExists(id) {
    const chart = Data.get(id);
    const exists = chart !== null && chart !== undefined;
    return exists;
}

export function resizeChart(id) {
    const chart = Data.get(id);
    if (chart) {
        chart.resize();
        chart.update('none');
    }
}

export function dispose(id) {
    Data.remove(id)
}
