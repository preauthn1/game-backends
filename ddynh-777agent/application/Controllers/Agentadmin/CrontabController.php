<?php
namespace App\Controllers\Agentadmin;

class CrontabController extends BaseController
{
    /**
     * crontab要加
     * * * * *  /usr/bin/curl "http://***.com/schedule/minute" &> /dev/null
     * /15* * * *  /usr/bin/curl "http://***.com/schedule/quarter" &> /dev/null
    0 * * * *  /usr/bin/curl "http://***com/schedule/hour" &> /dev/null
    0 0 * * *  /usr/bin/curl "http://***com/schedule/day" &> /dev/null
     */
    public function actionSchedule(){
        $time = $this->request->getAttribute('time');
        switch ($time){
            case 'day':$this->day();
                break;
            case 'hour':$this->hour();
                break;
            case 'quarter':$this->quarter();
                break;
            case 'minute':$this->minute();
                break;
        }
    }
    /*
     * minutely
     */
    private function minute()
    {
        // 获取当前的分数
        $minute = date('i');
        $hour = date('H');
        //每分钟执行一次
        set_time_limit(0);
        //更新玩家代理广告方案
        if($hour==3){
            if($minute==20){
                $api = new ApiController($this->container);
                $api->agentPlanSQL4();
            }else if($minute==30){
                $api = new ApiController($this->container);
                $api->agentPlanSQL3();
            }else if($minute==40){
                $api = new ApiController($this->container);
                $api->agentPlanSQL2();
            }else if($minute==50){
                $api = new ApiController($this->container);
                $api->agentPlanSQL1();
            }
        }

        // 每五分钟执行一次
        if ($minute % 5 == 3)
        {
        }

        // 每六分钟执行一次
        if ($minute % 6 == 0)
        {
        }

        //每五分钟执行一次
        if ($minute % 5 == 1)
        {
      /*      $api = new ApiController($this->container);
            $api->adRotation1();*/
        }
        //每五分钟执行一次
        if ($minute % 5 == 2)
        {
/*            $api = new ApiController($this->container);
            $api->adRotation2();*/
        }
        //每五分钟执行一次
        if ($minute % 5 == 3)
        {
/*            $api = new ApiController($this->container);
            $api->adRotation3();*/
        }
        //每五分钟执行一次
        if ($minute % 5 == 4)
        {
/*            $api = new ApiController($this->container);
            $api->adRotation4();*/
        }

        // 每半个小时执行一次
        if ($minute % 30 == 3)
        {
        }
    }


    private function quarter()
    {
        $minute = date('i');
        $hour = date('H');
        if($minute==15){
            if ($hour % 2 == 0)
            {
                $api = new ApiController($this->container);
                $api->agentReportDay();
            }
        }
    }

    /*
     * hourly
     */
    private function hour()
    {
        // 获取当前的小时数
        $hour = date('H');
        set_time_limit(0);
        //每小时执行一次

        //1点执行
        if ($hour == '1')
        {
        }

        //每两小时执行一次
        if ($hour%2 == 0)
        {
        }
    }

    /*
     * daily 00:00:00
     */
    private function day()
    {

    }

}
