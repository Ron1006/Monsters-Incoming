package com.RonLiu.MonstersIncoming;   // 用你的真实包名

import android.app.Application;
import com.google.android.gms.games.PlayGamesSdk;

public class GooglePlayGamesApplication extends Application {
    @Override
    public void onCreate() {
        super.onCreate();
        // 官方要求的初始化
        PlayGamesSdk.initialize(this);
    }
}
