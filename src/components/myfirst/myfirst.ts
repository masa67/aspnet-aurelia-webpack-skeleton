export class MyFirst {
    private firstName = 'John';
    private surName = 'Doe';
    private myTitle = 'green';

    constructor() {
        var self = this;
        setTimeout(function () {
            self.myTitle = 'red';
            alert('changing to red...');
        }, 3000);
    }

    sayHello() {
        alert(`Hello, ${this.firstName} ${this.surName}!`);
    }
}