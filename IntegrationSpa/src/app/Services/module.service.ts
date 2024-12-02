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
  DeleteModule(id) {
    return this.HttpClient.delete(
      environment.BaseUrl + '/Module/DeleteModule/' + id
    );
  }
  CreateModule(module) {
    return this.HttpClient.post(
      environment.BaseUrl + '/Module/CreateModule',
      module
    );
  }
  EditModule(module) {
    return this.HttpClient.post(
      environment.BaseUrl + '/Module/EditModule',
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
