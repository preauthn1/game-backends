#!/bin/bash
#<?php die(); ?>
source /etc/profile
umask 002
#切换到当前目录
path=$0
path=`/usr/bin/dirname $path`
#cd $(cd "$(dirname "$0")";pwd)
readonly path=${path}/;
readonly minute=$((10#$(date +%M)))
readonly hour=$(date +%k)
readonly day=$((10#$(date +%d)))
readonly month=$(date +%m)
readonly weekday=$(date +%w)
readonly Ymd=$(date +%Y%m%d)

#每1分钟执行的任务

#每5分钟执行的任务
if [ "3" -eq "$(($minute % 5))" ]; then
    /opt/php/bin/php -f ${path}cli.php CronCliController insertByPerGameLog > /dev/null 2>&1 &   #牌局
    /opt/php/bin/php -f ${path}cli.php CronCliController setAdRotation > /dev/null 2>&1 &   #设置广告定时轮换方案
fi

#每2分钟执行的任务
if [ "0" -eq "$(($minute % 2))" ]; then
    /opt/php/bin/php -f ${path}cli.php AutoController actionPayExchange > /dev/null 2>&1 &   #自动打款
fi

#每2分钟执行的任务
if [ "1" -eq "$(($minute % 2))" ]; then
    /opt/php/bin/php -f ${path}cli.php AutoController actionPayExchangeQuery > /dev/null 2>&1 &   #自动打款查询
fi

#每3分钟执行的任务
if [ "0" -eq "$(($minute % 3))" ]; then
    /opt/php/bin/php -f ${path}cli.php CronCliController handleNotDeLiver > /dev/null 2>&1 &   #发货
fi

#每10分钟执行的任务
if [ "5" -eq "$(($minute % 10))" ]; then
    /opt/php/bin/php -f ${path}cli.php CronCliController paySuccessRateMonitor > /dev/null 2>&1 &   #下单成功率监控
fi

#每30分钟执行的任务，每小时第2和32分钟执行
if [ "2" -eq "$(($minute % 30))" ]; then
    /opt/php/bin/php -f ${path}cli.php StatCliController online > /dev/null 2>&1 &   #在线统计
    /opt/php/bin/php -f ${path}cli.php CronCliController backGold > /dev/null 2>&1 &   #兑换失败延迟退款
fi

#每1小时执行的任务，每小时第0分钟执行
if [ "0" -eq "$minute" ] ; then
    /opt/php/bin/php -f ${path}cli.php StatCliController dayDetailReport > /dev/null 2>&1 &   #系统日报
    /opt/php/bin/php -f ${path}cli.php StatCliController channelDetailReport > /dev/null 2>&1 &  #系统渠道日报
fi

#每天凌晨4点10分执行
if [ "4" -eq $hour ] && [ "10" -eq $minute ] ; then
/opt/php/bin/php -f ${path}cli.php CronCliController setUserAdPlan > /dev/null 2>&1 &   #设置用户的广告方案
fi

#每2小时0分执行
if [ "0" -eq $minute ] && [ "0" -eq "$(($hour % 2))" ]; then
/opt/php/bin/php -f ${path}cli.php CronCliController agentReportDay > /dev/null 2>&1 &   #代理日报
fi
