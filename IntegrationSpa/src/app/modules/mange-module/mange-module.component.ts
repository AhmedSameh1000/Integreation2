import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { NgIf } from '@angular/common';
import {
  Component,
  ElementRef,
  Inject,
  OnInit,
  ViewChild,
} from '@angular/core';
import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { ToastrService } from 'ngx-toastr';
import { DataBaseService } from 'src/app/Services/data-base.service';
import { ModuleService } from 'src/app/Services/module.service';
import { __values } from 'tslib';

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
    private MatdiloRef: MatDialogRef<MangeModuleComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { Id: string }
  ) {}

  ngOnInit(): void {
    this.initializeForm();
    this.GetDataBases();
    if (this.data) {
      this.GetModuleById();
    }
  }

  ngAfterViewInit() {
    this.Element = document.getElementById('Sourceherer');
  }

  AddReferance(source: string, local: string, primary: string) {
    // Create the form group with selected values
    var Reference = new FormGroup({
      TableFromName: new FormControl(source, Validators.required),
      LocalName: new FormControl(local, Validators.required),
      PrimaryName: new FormControl(primary, Validators.required),
    });

    if (Reference.invalid) {
      this.toastr.error('Error Invalid data');
      return;
    }
    // Push the form group to the References FormArray
    let References = this.moduleForm.get('References') as FormArray;
    References.push(Reference);
    Reference.reset();
  }

  isDisabled = true;
  moduleForm: FormGroup;

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

  AddModule() {
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
  SaveModule() {
    console.log(this.moduleForm.value);
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

  GetModuleById() {
    this.manageModuleService.GetModuleById(this.data.Id).subscribe({
      next: (res: any) => {
        this.GetInfo(res);
        this.mapModuleToForm(res.data);
        console.log(res.data);
      },
    });
  }
  GetInfo(res) {
    this.ToDataBaseSelectedId = res.data.toDbId;
    this.FromDataBaseSelectedId = res.data.fromDbId;
    this.DataBaseService.GetTables(this.FromDataBaseSelectedId).subscribe({
      next: (tablefrom: any) => {
        this.Fromtables = tablefrom;
      },
    });
    this.DataBaseService.GetTables(this.ToDataBaseSelectedId).subscribe({
      next: (tableto: any) => {
        this.Totables = tableto;
      },
    });

    this.DataBaseService.GetColumns(
      this.FromDataBaseSelectedId,
      res.data.tableFromName
    ).subscribe({
      next: (columnfrom) => {
        this.ColumnsFrom = columnfrom;
      },
    });
    this.DataBaseService.GetColumns(
      this.ToDataBaseSelectedId,
      res.data.tableToName
    ).subscribe({
      next: (ColumnTo) => {
        this.ColumnsTo = ColumnTo;
      },
    });

    // this.DataBaseService.GetColumns(this.ToDataBaseSelectedId)
  }
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
  mapModuleToForm(moduleData: any) {
    // Set simple form controls
    this.moduleForm = new FormGroup({
      moduleName: new FormControl(moduleData.name, Validators.required),
      tableFromName: new FormControl(
        moduleData.tableFromName,
        Validators.required
      ),
      tableToName: new FormControl(moduleData.tableToName, Validators.required),
      toPrimaryKeyName: new FormControl(
        moduleData.toPrimaryKeyName,
        Validators.required
      ),
      fromPrimaryKeyName: new FormControl(
        moduleData.fromPrimaryKeyName,
        Validators.required
      ),
      localIdName: new FormControl(moduleData.localIdName, Validators.required),
      cloudIdName: new FormControl(moduleData.cloudIdName, Validators.required),
      toDbId: new FormControl(moduleData.toDbId, Validators.required),
      fromDbId: new FormControl(moduleData.fromDbId, Validators.required),
      toInsertFlagName: new FormControl(
        moduleData.toInsertFlagName,
        Validators.required
      ),
      toUpdateFlagName: new FormControl(
        moduleData.toUpdateFlagName,
        Validators.required
      ),
      fromInsertFlagName: new FormControl(
        moduleData.fromInsertFlagName,
        Validators.required
      ),
      fromUpdateFlagName: new FormControl(
        moduleData.fromUpdateFlagName,
        Validators.required
      ),
      Columns: new FormArray([]),
      References: new FormArray([]),
    });
    const columnsArray = this.moduleForm.get('Columns') as FormArray;
    moduleData.columnsFromDTOs.forEach((column: any) => {
      columnsArray.push(
        new FormGroup({
          ColumnFrom: new FormControl(
            column.columnFromName,
            Validators.required
          ),
          ColumnTo: new FormControl(column.columnToName, Validators.required),
          IsChecked: new FormControl(column.isReference, Validators.required),
          Referance: new FormControl(column.tableReferenceName),
        })
      );
    });

    this.GetColumnsReferance(moduleData.referancesForReturnDTOs);
  }

  Element: any;
  GetColumnsReferance(referancesForReturnDTOs) {
    const Referances = this.moduleForm.get('References') as FormArray;

    referancesForReturnDTOs.forEach((column: any) => {
      Referances.push(
        new FormGroup({
          TableFromName: new FormControl(column.tableFromName),
          LocalName: new FormControl(column.localName),
          PrimaryName: new FormControl(column.primaryName),
        })
      );
    });
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
}
