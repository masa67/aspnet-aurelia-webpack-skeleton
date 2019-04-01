export class Greeting {
    private message = "Hello, world!";
    selectedFiles!:FileList;

    inputChange() {
        let image = new Image();
        
        image.onload = function () {
            let elem = this as HTMLImageElement;

            let canvas = <HTMLCanvasElement> document.createElement('canvas');

            canvas.width = elem.naturalWidth;
            canvas.height = elem.naturalHeight;

            let context = <CanvasRenderingContext2D> canvas.getContext('2d');

            context.drawImage(elem, 0, 0);

            let stringImg = canvas.toDataURL('image/png').replace(/^data:image\/(png|jpg);base64,/, '');
        };

        let file = this.selectedFiles.item(0);
        if (file !== null)
            image.src = URL.createObjectURL(file);
    }
}
