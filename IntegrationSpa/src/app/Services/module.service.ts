import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment.development';

@Injectable({
  providedIn: 'root',
})
export class ModuleService {
  constructor(private HttpClient: HttpClient) {}

  GetModules() {
    return this.HttpClient.get(environment.BaseUrl + '/Module/Modules');
  }
  GetModuleById(id) {
    return this.HttpClient.get(environment.BaseUrl + '/Module/GetModule/' + id);
  }
  CreateModule(module) {
    return this.HttpClient.post(
      environment.BaseUrl + '/Module/CreateModule',
      module
    );
  }

  Sync(Id, SyncType) {
    return this.HttpClient.get(
      environment.BaseUrl +
        '/Module/Sync?ModuleId=' +
        Id +
        '&syncType=' +
        SyncType
    );
  }
}
