import * as $ from "jquery";
import "bootstrap";
import "@dashboardcode/bsmultiselect";

export class MyFirst {
    selectElementRef: HTMLElement;

    attached() {
        let elems = $(this.selectElementRef).find("select[multiple='multiple']");
        elems.bsMultiSelect({
            selectedPanelDefMinHeight: 'calc(2.25rem + 2px)',  // default size
            selectedPanelLgMinHeight: 'calc(2.875rem + 2px)',  // LG size
            selectedPanelSmMinHeight: 'calc(1.8125rem + 2px)', // SM size
            selectedPanelDisabledBackgroundColor: '#e9ecef',   // disabled background
            selectedPanelFocusBorderColor: '#80bdff',          // focus border
            selectedPanelFocusBoxShadow: '0 0 0 0.2rem rgba(0, 123, 255, 0.25)',  // foxus shadow
            selectedPanelFocusValidBoxShadow: '0 0 0 0.2rem rgba(40, 167, 69, 0.25)',  // valid foxus shadow
            selectedPanelFocusInvalidBoxShadow: '0 0 0 0.2rem rgba(220, 53, 69, 0.25)',  // invalid foxus shadow
            inputColor: '#495057', // color of keyboard entered text
            selectedItemContentDisabledOpacity: '.65' // btn disabled opacity used
        });
    }
}