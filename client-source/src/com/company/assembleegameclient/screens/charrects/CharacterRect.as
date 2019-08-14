package com.company.assembleegameclient.screens.charrects {
import com.company.rotmg.graphics.StarGraphic;

import flash.display.Shape;
import flash.display.Sprite;
import flash.events.MouseEvent;
import flash.filters.DropShadowFilter;
import flash.geom.ColorTransform;

import io.decagames.rotmg.ui.sliceScaling.SliceScalingBitmap;
import io.decagames.rotmg.ui.texture.TextureParser;
import io.decagames.rotmg.utils.colors.Tint;

import kabam.rotmg.text.view.TextFieldDisplayConcrete;
import kabam.rotmg.text.view.stringBuilder.StringBuilder;

public class CharacterRect extends Sprite {

    public static const WIDTH:int = 600;
    public static const HEIGHT:int = 70;

    public var color:uint;
    public var overColor:uint;
    private var box:SliceScalingBitmap;
    protected var taglineIcon:Sprite;
    protected var taglineText:TextFieldDisplayConcrete;
    protected var classNameText:TextFieldDisplayConcrete;
    protected var className:StringBuilder;
    public var selectContainer:Sprite;

    public function CharacterRect() {
        super();
    }

    private static function makeDropShadowFilter():Array {
        return ([new DropShadowFilter(0, 0, 0, 1, 8, 8)]);
    }


    public function init():void {
        tabChildren = false;
        this.makeBox();
        this.makeContainer();
        this.makeClassNameText();
        this.addEventListeners();
    }

    private function addEventListeners():void {
        addEventListener(MouseEvent.MOUSE_OVER, this.onMouseOver);
        addEventListener(MouseEvent.ROLL_OUT, this.onRollOut);
    }

    public function makeBox():void {
        this.box = TextureParser.instance.getSliceScalingBitmap("UI", "popup_header_title", WIDTH);
        this.box.height = HEIGHT;
        this.box.x = 0;
        this.box.y = 0;
        addChild(this.box);
    }

    protected function onMouseOver(_arg_1:MouseEvent):void {
        this.drawBox(true);
    }

    protected function onRollOut(_arg_1:MouseEvent):void {
        this.drawBox(false);
    }

    private function drawBox(_arg_1:Boolean):void {
        if (_arg_1){
            Tint.add(this.box, 0xC9C9C9, 0.2);
        }
        else {
            this.box.transform.colorTransform = new ColorTransform();
        };
        this.box.scaleX = 1;
        this.box.scaleY = 1;
        this.box.x = 0;
        this.box.y = 0;
    }

    public function makeContainer():void {
        this.selectContainer = new Sprite();
        this.selectContainer.mouseChildren = false;
        this.selectContainer.buttonMode = true;
        this.selectContainer.graphics.beginFill(0xFF00FF, 0);
        this.selectContainer.graphics.drawRect(0, 0, WIDTH, HEIGHT);
        addChild(this.selectContainer);
    }

    protected function makeTaglineIcon():void {
        this.taglineIcon = new StarGraphic();
        this.taglineIcon.transform.colorTransform = new ColorTransform((179 / 0xFF), (179 / 0xFF), (179 / 0xFF));
        this.taglineIcon.scaleX = 1.2;
        this.taglineIcon.scaleY = 1.2;
        this.taglineIcon.x = CharacterRectConstants.TAGLINE_ICON_POS_X + 90;
        this.taglineIcon.y = CharacterRectConstants.TAGLINE_ICON_POS_Y + 10;
        this.taglineIcon.filters = [new DropShadowFilter(0, 0, 0)];
        this.selectContainer.addChild(this.taglineIcon);
    }

    protected function makeClassNameText():void {
        this.classNameText = new TextFieldDisplayConcrete().setSize(18).setColor(0xFFFFFF);
        this.classNameText.setBold(true);
        this.classNameText.setStringBuilder(this.className);
        this.classNameText.filters = makeDropShadowFilter();
        this.classNameText.x = CharacterRectConstants.CLASS_NAME_POS_X + 90;
        this.classNameText.y = CharacterRectConstants.CLASS_NAME_POS_Y + 10;
        this.selectContainer.addChild(this.classNameText);
    }

    protected function makeTaglineText(_arg_1:StringBuilder):void {
        this.taglineText = new TextFieldDisplayConcrete().setSize(14).setColor(0xB3B3B3);
        this.taglineText.setStringBuilder(_arg_1);
        this.taglineText.filters = makeDropShadowFilter();
        this.taglineText.x = CharacterRectConstants.TAGLINE_TEXT_POS_X + 90;
        this.taglineText.y = CharacterRectConstants.TAGLINE_TEXT_POS_Y + 10;
        this.selectContainer.addChild(this.taglineText);
    }


}
}//package com.company.assembleegameclient.screens.charrects
