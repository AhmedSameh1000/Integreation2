import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { Component, OnInit } from '@angular/core';
import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { DataBaseService } from 'src/app/Services/data-base.service';
import { ModuleService } from 'src/app/Services/module.service';

@Component({
  selector: 'app-mange-module',
  templateUrl: './mange-module.component.html',
  styleUrls: ['./mange-module.component.css'],
})
export class MangeModuleComponent implements OnInit {
  constructor(
    private manageModuleService: ModuleService,
    private toastr: ToastrService,
    private DataBaseService: DataBaseService,
    private MatdiloRef: MatDialogRef<MangeModuleComponent>
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.GetDataBases();
  }
  AddColumn() {
    var Column = new FormGroup({
      ColumnTo: new FormControl(null),
      ColumnFrom: new FormControl(null),
      IsChecked: new FormControl(false),
      Referance: new FormControl(null),
    });

    let Columns = this.moduleForm.get('Columns') as FormArray;

    Columns.push(Column);
  }

  AddReferance() {
    var Reference = new FormGroup({
      TableFromName: new FormControl(null),
      LocalName: new FormControl(null),
      PrimaryName: new FormControl(null),
    });

    let References = this.moduleForm.get('References') as FormArray;

    References.push(Reference);
  }
  moduleForm: FormGroup;

  initializeForm() {
    this.moduleForm = new FormGroup({
      moduleName: new FormControl('', Validators.required),
      tableFromName: new FormControl('', Validators.required),
      tableToName: new FormControl('', Validators.required),
      toPrimaryKeyName: new FormControl('', Validators.required),
      fromPrimaryKeyName: new FormControl('', Validators.required),
      localIdName: new FormControl('', Validators.required),
      cloudIdName: new FormControl('', Validators.required),
      toDbId: new FormControl('', Validators.required),
      fromDbId: new FormControl('', Validators.required),
      toInsertFlagName: new FormControl('', Validators.required),
      toUpdateFlagName: new FormControl('', Validators.required),
      fromInsertFlagName: new FormControl('', Validators.required),
      fromUpdateFlagName: new FormControl('', Validators.required),
      Columns: new FormArray([]),
      References: new FormArray([]),
    });
  }

  DataBases: any[] = []; // قائمة قواعد البيانات اللي هنعرضها في الـ select

  // جلب قواعد البيانات من الـ API
  GetDataBases() {
    this.DataBaseService.GetDataBases().subscribe({
      next: (res: any) => {
        this.DataBases = res; // تخزين بيانات قواعد البيانات في المتغير
        console.log(this.DataBases);
      },
      error: (err) => {
        console.error('Error fetching databases:', err);
      },
    });
  }
  FromDataBaseSelected = false;
  ToDataBaseSelected = false;

  onSubmit() {
    // if (this.moduleForm.valid) {
    //   this.manageModuleService.saveModule(this.moduleForm.value).subscribe({
    //     next: (res) => {
    //       this.toastr.success('Module Saved Successfully');
    //       // Handle any additional logic like closing the dialog or navigating
    //     },
    //     error: (err) => {
    //       this.toastr.error('Failed to Save Module');
    //     }
    //   });
    // }
  }
  Fromtables: any;
  FromDataBaseSelectedId: any;
  OnDbFromChange(event) {
    var databaseId = event.target.value;
    this.FromDataBaseSelectedId = databaseId;
    this.DataBaseService.GetTables(databaseId).subscribe({
      next: (res) => {
        this.Fromtables = res;
      },
      error: (err) => {
        console.log(err);
      },
    });
  }
  ColumnsFrom: any;
  OnTableFromChange(event) {
    var TableName = event.target.value;
    this.DataBaseService.GetColumns(
      this.FromDataBaseSelectedId,
      TableName
    ).subscribe({
      next: (res) => {
        console.log(res);
        this.ColumnsFrom = res;
      },
      error: (err) => {
        console.log(err);
      },
    });
  }

  Totables: any;
  ToDataBaseSelectedId: any;
  OnDbToChange(event) {
    var databaseId = event.target.value;
    this.ToDataBaseSelectedId = databaseId;
    this.DataBaseService.GetTables(databaseId).subscribe({
      next: (res) => {
        this.Totables = res;
      },
      error: (err) => {
        console.log(err);
      },
    });
  }
  ColumnsTo: any;
  OnTableChange(event) {
    var TableName = event.target.value;
    this.DataBaseService.GetColumns(
      this.ToDataBaseSelectedId,
      TableName
    ).subscribe({
      next: (res) => {
        console.log(res);
        this.ColumnsTo = res;
      },
      error: (err) => {
        console.log(err);
      },
    });
  }
  selectedTabIndex = 0;

  nextTab() {
    if (this.selectedTabIndex >= 3) return;
    this.selectedTabIndex++;
  }
  Prevtab() {
    if (this.selectedTabIndex <= 0) return;

    this.selectedTabIndex--;
  }

  Print() {
    console.log(this.moduleForm.value);
    this.manageModuleService.CreateModule(this.moduleForm.value).subscribe({
      next: (res) => {
        console.log(res);
        this.toastr.success('Module Created Sucessfuly');
        this.MatdiloRef.close(true);
      },
      error: (err: any) => {
        this.toastr.error('An Error When Create Module');
        console.log(err);
      },
    });
  }
  get Columns() {
    return this.moduleForm.get('Columns') as FormArray;
  }
  removeColumn(index: number) {
    this.Columns.removeAt(index);
  }
  OnRefranceTableChange(
    event: Event,
    LocalSelected: HTMLSelectElement,
    primarySelect: HTMLSelectElement
  ) {
    const Table = (event.target as HTMLSelectElement).value;

    // Clear existing options for both LocalSelected and primarySelect
    LocalSelected.innerHTML = '';
    primarySelect.innerHTML = '';

    // Add default placeholder options
    const localOptionPlaceholder = document.createElement('option');
    localOptionPlaceholder.value = '';
    localOptionPlaceholder.text = 'Select Column';
    localOptionPlaceholder.disabled = true;
    localOptionPlaceholder.selected = true;

    const primaryOptionPlaceholder = document.createElement('option');
    primaryOptionPlaceholder.value = '';
    primaryOptionPlaceholder.text = 'Select Column';
    primaryOptionPlaceholder.disabled = true;
    primaryOptionPlaceholder.selected = true;

    LocalSelected.appendChild(localOptionPlaceholder);
    primarySelect.appendChild(primaryOptionPlaceholder);

    // Fetch columns and populate the select elements
    this.DataBaseService.GetColumns(this.ToDataBaseSelectedId, Table).subscribe(
      {
        next: (res: string[]) => {
          res.forEach((column) => {
            // Create and append options for LocalSelected
            const localOption = document.createElement('option');
            localOption.value = column;
            localOption.text = column;
            LocalSelected.appendChild(localOption);

            // Create and append options for primarySelect
            const primaryOption = document.createElement('option');
            primaryOption.value = column;
            primaryOption.text = column;
            primarySelect.appendChild(primaryOption);
          });
        },
        error: (err) => {
          console.error('Error fetching columns:', err);
          // Optionally, you can display an error message to the user
        },
      }
    );
  }
}
