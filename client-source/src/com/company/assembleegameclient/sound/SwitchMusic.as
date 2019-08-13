package com.company.assembleegameclient.sound {
import com.company.assembleegameclient.parameters.Parameters;
import com.gskinner.motion.GTween;

import flash.events.IOErrorEvent;
import flash.media.Sound;
import flash.media.SoundChannel;
import flash.media.SoundTransform;
import flash.net.URLRequest;

import kabam.rotmg.application.api.ApplicationSetup;
import kabam.rotmg.core.StaticInjectorContext;

public class SwitchMusic {

    private var music:Sound;
    private var musicChannel:SoundChannel;
    private var musicTransform:SoundTransform;
    private var gT:GTween;

    public function switchMusic(musicName:String):void {
        var appSetup:ApplicationSetup = StaticInjectorContext.getInjector().getInstance(ApplicationSetup);
        var musicPath:String = appSetup.getAppEngineUrl(true) + "/music/";
        music = new Sound();
        music.addEventListener(IOErrorEvent.IO_ERROR, defaultMusic);
        musicTransform = new SoundTransform(0);
        gT = new GTween(musicTransform);
        gT.onChange = updateVolume;
        music.load(new URLRequest(musicPath + musicName + ".mp3"));
    }

    public function fadeIn():void {
        gT.duration = 3;
        gT.setValue("volume", Parameters.data_.musicVolume);
        musicChannel = music.play(0, int.MAX_VALUE, musicTransform)
    }

    public function fadeOut():void {
        gT.onComplete = stopChannel;
        gT.setValue("volume", 0)
    }

    public function changeVolume(volume:Number):void {
        musicTransform.volume = volume;
        musicChannel.soundTransform = musicTransform;
    }

    private function updateVolume(gtween:GTween):void {
        musicChannel.soundTransform = musicTransform;
    }

    private function stopChannel(gtween:GTween):void {
        musicChannel.stop();
    }

    private static function defaultMusic(_arg_1:IOErrorEvent):void {
        Music.chooseMusic("default");
    }


}
}