<?php

namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class PerGameDetailLogModel extends Model
{
    protected $connection = 'perGameLog';
    protected $table = 'PerGameLog.PerLog.PerGameDetailLog';

    /**
     * 脚本任务，定时从PerGameLog表跑数据到此表
     * @return bool
     */
    public function insertByPerGameLog() {
        $limit = 1000;//每次操作100条，5分钟执行一次
        $maxid = PerGameDetailLogModel::max("PerGameLogID");
        $maxid = intval($maxid) > 0 ? intval($maxid) : 0;
        $list = PerGameLogModel::where("Id", ">", $maxid)->orderBy('Id')->limit($limit)->get()->toArray();
        if ($list) {
            foreach ($list as $k => $v) {
                $PerGameLogID = $v['Id'];
                $ServerID = $v['ServerID'];
                $serverInfo = GameServerListModel::where('ServerID', $ServerID)->first();
                $ServerName = $serverInfo ? $serverInfo['ServerName'] : "";
                $GameID = $serverInfo ? $serverInfo['GameID'] : "";
                list($name, $GameLevel) = explode("_", $ServerName);
                $GameLevel = intval($GameLevel);
                $GameData = $v['GameData'];
                $TaxAmount = $v['TaxAmount'];
                $BeginTime = $v['BeginTime'];
                $EndTime = $v['EndTime'];
                $CreateTime = \Carbon\Carbon::now();
                $gameDataArr = explode(PHP_EOL, $GameData);
                $begin = "";
                $end = "";
                foreach ($gameDataArr as $kk => $vv) {
                    if (strstr($vv, '游戏开始')) {
                        $begin = trim($vv, ',');
                    } elseif(strstr($vv, '游戏结束')) {
                        $end = trim($vv, ',');
                    }
                }
                //处理结束
                $end = str_replace("游戏结束", 'gameend', $end);
                $end = substr($end, strpos($end,"gameend.") + 8);
                $endUserArr = $end ? array_filter(explode("userid", $end)) : [];
                $changeAmountArr = [];
                foreach ($endUserArr as $kk => $vv) {
                    $vv = "userid" . $vv;
                    list($user, $winamount) = explode(",", $vv);
                    list($tmp, $uid) = explode(":", $user);
                    list($tmp, $winCount) = explode(":", $winamount);
                    $changeAmountArr[$uid] = $winCount;
                }
                //处理开始
                $begin = substr($begin, strpos($begin,"桌子号:"));
                list($tableNoInfo, $userInfoStr) = explode(".", $begin);
                list($tmp, $TableNo) = explode(":", $tableNoInfo);
                //$userInfo = str_replace(['userid','seat','score'],['用户id','座位号','初始分数'], $userInfo);
                $userInfoArr = $userInfoStr ? array_filter(explode("userid", $userInfoStr)) : array();
                if (!$userInfoArr) continue;
                foreach ($userInfoArr as $kk => $vv) {
                    $vv = "userid".$vv;
                    list($user, $seat, $score) = explode(",", $vv);
                    list($tmp, $UserID) = explode(":", $user);
                    list($tmp, $Seat) = explode(":", $seat);
                    list($tmp, $PreAmount) = explode(":", $score);
                    $ChangeAmount = isset($changeAmountArr[$UserID]) ? $changeAmountArr[$UserID] : 0;
                    $insertData = [
                        'PerGameLogID' => $PerGameLogID,
                        'ServerID' => $ServerID,
                        'ServerName' => $ServerName,
                        'GameID' => $GameID,
                        'TableNo' => $TableNo,
                        'UserID' => $UserID,
                        'GameLevel' => $GameLevel,
                        'Seat' => $Seat,
                        'PreAmount' => $PreAmount,
                        'ChangeAmount' => $ChangeAmount,
                        'TaxAmount' => $TaxAmount,
                        'GameData' => '',
                        'BeginTime' => $BeginTime,
                        'EndTime' => $EndTime,
                        'CreateTime' => $CreateTime,
                    ];
                    PerGameDetailLogModel::insert($insertData);
                }
            }
        }
        return true;
    }
}