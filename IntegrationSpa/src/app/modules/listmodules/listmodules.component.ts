import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { ModuleService } from 'src/app/Services/module.service';
import { MangeModuleComponent } from '../mange-module/mange-module.component';

@Component({
  selector: 'app-listmodules',
  templateUrl: './listmodules.component.html',
  styleUrls: ['./listmodules.component.css'],
})
export class ListmodulesComponent implements OnInit {
  Modules: any[] = [];

  constructor(
    private ModuleService: ModuleService,
    private toastr: ToastrService,
    private MatDilog: MatDialog
  ) {}

  ngOnInit(): void {
    this.GetModules();
  }

  GetModules() {
    this.ModuleService.GetModules().subscribe({
      next: (res: any[]) => {
        this.Modules = res.map((module) => ({ ...module, isLoading: false }));
        console.log(res);
      },
    });
  }

  syncModule(moduleId: number, syncType: any) {
    // العثور على العنصر الذي يتم مزامنته حاليا وتحديث حالة التحميل الخاصة به
    const module = this.Modules.find((mod) => mod.id === moduleId);
    if (module) {
      module.isLoading = true;
      console.log(`Syncing module with ID: ${moduleId}`);
      this.ModuleService.Sync(moduleId, syncType).subscribe({
        next: (res: any) => {
          console.log(res);
          module.isLoading = false; // إيقاف التحميل للعنصر المحدد فقط
          this.toastr.success(res.data + ' ' + res.message);
        },
        error: (err) => {
          console.log(err);
          module.isLoading = false; // إيقاف التحميل في حالة حدوث خطأ للعنصر المحدد فقط

          this.toastr.error(err.error.message);
        },
      });
    }
  }

  MangeModule() {
    var Dilog = this.MatDilog.open(MangeModuleComponent, {
      width: '60%',
      disableClose: true,
    });
    Dilog.afterClosed().subscribe({
      next: (res) => {
        if (res) this.GetModules();
      },
    });
  }
}
