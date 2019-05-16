import * as $ from "jquery";

export class MyFirst {
    private firstName = 'John';
    private surName = 'Doe';

    sayHello() {
        // Normally, we would not do this, but this is to test use of jquery.
        $('.message').html(`Hello, ${this.firstName} ${this.surName}!`);
    }
}