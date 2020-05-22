import { Component, EventEmitter, NgModule, Output, OnInit } from '@angular/core';
import { MdButtonModule, MdIconModule } from '@angular/material';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css']
})
export class HeaderComponent implements OnInit {

  constructor(/*public _componentPageTitle: ComponentPageTitle*/) { }

  @Output() toggleSidenav = new EventEmitter<void>();

  ngOnInit() {
  }


  getTitle() {
    return "!!!!"; //this._componentPageTitle.title;
  }
}