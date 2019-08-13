package com.company.assembleegameclient.sound {
import com.company.assembleegameclient.parameters.Parameters;

public class Music {

    private static var originalMusic:String;
    private static var music_:SwitchMusic;

    public static function load():void {
    }

    public static function chooseMusic(music:String):void {
        if (originalMusic == music) return;
        originalMusic = music;
        if (music_) {
            music_.fadeOut();
        }
        music_ = new SwitchMusic();
        music_.switchMusic(music);
        music_.fadeIn();
    }

    public static function setPlayMusic(_arg_1:Boolean):void {
        Parameters.data_.playMusic = _arg_1;
        Parameters.save();

    }

    public static function setMusicVolume(_arg_1:Number):void {
        Parameters.data_.musicVolume = _arg_1;
        Parameters.save();
        if (!Parameters.data_.playMusic) {
            return;
        }
        music_.changeVolume(_arg_1);
    }


}
}//package com.company.assembleegameclient.sound