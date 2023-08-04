package com.damy.apkagent;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;

/**
 * Created with IntelliJ IDEA.
 * User: Administrator
 * Date: 13-10-25
 * Time: 上午9:51
 * To change this template use File | Settings | File Templates.
 */
public class CompletedAlarmActivity extends Activity {
    private Button btn_confirm = null;

    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_completed_alarm);
        btn_confirm = (Button)findViewById(R.id.completed_alert_confirm_button);
        btn_confirm.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                finish();
            }
        });
    }
}