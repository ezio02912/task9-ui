import DataService from '../_content/BootstrapBlazor.UniverSheet/data-service.js'
import { ReportController } from './controller.js'

const { Plugin, Injector, setDependencies } = UniverCore;

export class ReportPlugin extends Plugin {
    static pluginName = 'ReportPlugin';

    constructor(_injector) {
        super();

        this._injector = _injector;
    }

    onStarting() {
        this._injector.add([ReportController]);
        this._injector.add([DataService.name, { useClass: DataService }])
    }

    onReady() {
        this._injector.get(ReportController)
    }

    onRendered() {

    }
}

setDependencies(ReportPlugin, [Injector]);
