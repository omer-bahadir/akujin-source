package kabam.rotmg.ui.view.components {
import flash.display.Shape;

public class DarkLayer extends Shape {

    public function DarkLayer() {
        graphics.beginFill(0x262741, 1);
        graphics.drawRect(0, 0, 800, 600);
        graphics.endFill();
    }

}
}//package kabam.rotmg.ui.view.components
