//   templateUrl: './checkbox-list.component.html',
//   styleUrl: './checkbox-list.component.css'

// checkbox-list.component.ts
import { Component, Input, Output, EventEmitter, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common'; // Import CommonModule
import { AvStream } from '../file-quality-picker.component';

@Component({
  imports: [CommonModule], 
  selector: 'app-checkbox-list',
  standalone: true,
  template: `
    <h2>{{ listTitle }}</h2>
    <ul>
      <li *ngFor="let item of this.items; let i = index">
        <input type="checkbox" [id]="getCheckboxId(item)" [checked]="isSelected(item.hashId)" (change)="onCheckboxChange(item.hashId, $event)" />
        <label [for]="getCheckboxId(item)">{{ item.friendlyName }}</label>
      </li>
    </ul>
  `,
  styles: []
})
export class CheckboxListComponent implements OnInit {
  @Input() items: AvStream[] = []; 
  @Input() listTitle: string = "Select Items";
  selectedItemsHashIds: string[] = []; 
  @Output() selectedItemsChange = new EventEmitter<string[]>();

  ngOnInit(): void {
    // Initialize selectedItems if needed (e.g., from local storage)
    // Example: this.selectedItems = JSON.parse(localStorage.getItem('selectedItems') || '[]');
      console.log("ngOnInit");
  }

  // ngOnChanges(changes: SimpleChanges): void {
  //     //if (changes['items'] && changes['items'].currentValue) {
  //       //this.selectedItems = this.selectedItems.filter(item => changes['items'].currentValue.includes(item));
  //       console.log("ngOnChanges");
  //     //}
  // }

  getCheckboxId(item: AvStream): string {
    //return `chb-${this.listTitle.replaceAll(' ', '') }-${index}`; 
    return item.hashId;
  }

  onCheckboxChange(itemHashId: string, event: Event) {
    const isChecked = (event.target as HTMLInputElement).checked;

    let newSelectedItems: string[];

    if (isChecked) {
      newSelectedItems = [...this.selectedItemsHashIds, itemHashId];
    } else {
      newSelectedItems = this.selectedItemsHashIds.filter(i => i != itemHashId);
    }

    this.selectedItemsHashIds = newSelectedItems;
    this.selectedItemsChange.emit(newSelectedItems);

    console.log("checkbox component selected \nnewSelectedItems   \n" + newSelectedItems);
    console.log("checkbox component selected \nthis.selectedItems   \n" + this.selectedItemsHashIds);
    console.log("checkbox component all \nthis.items   \n" + this.items);
  }

  isSelected(HashId: string): boolean {
    return this.selectedItemsHashIds.includes(HashId);
  }
}