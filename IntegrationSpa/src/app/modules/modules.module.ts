import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ModulesRoutingModule } from './modules-routing.module';
import { ListmodulesComponent } from './listmodules/listmodules.component';
import { SharedModule } from '../shared/shared.module';
import { MangeModuleComponent } from './mange-module/mange-module.component';

@NgModule({
  declarations: [ListmodulesComponent, MangeModuleComponent],
  imports: [CommonModule, ModulesRoutingModule, SharedModule],
})
export class ModulesModule {}
