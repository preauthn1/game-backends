<?php
namespace App\Models;

use Illuminate\Database\Eloquent\Model;

class MenuModel extends Model
{
    protected $table = 'menu';
    //protected static $returnData = [];
    protected $fillable = ['name', 'parent', 'url', 'type', 'sort'];
    public $timestamps  = false;

    public static function getMenu($data, $type = 1, $getTree = false)
    {
        foreach ($data as $key => $value) {
            if ($type == 0) {
                $treeData[$value['id']] = $value->toArray();
            } elseif ($value['type'] == $type) {
                if ($type == 2) {
                    $treeData[$value['id']] = $value->toArray();
                } else {
                    $treeData[] = $value->toArray();
                }

            }
        }
        if ($getTree) {
            $returnData = self::getTree($treeData);
        } else {
            $returnData = $treeData;
        }

        return $returnData;
    }

    private static function getTree($data)
    {
        //print_r($data);
        $result = [];
        $arr    = [];
        foreach ($data as $val) {
            if ($val['parent'] == 0) {
                $i               = count($result);
                $result[$i]      = $val;
                $arr[$val['id']] = &$result[$i];
            } else {
                if (!isset($arr[$val['parent']]['sub'])) {
                    $arr[$val['parent']]['sub'] = [];
                }
                $i                              = count($arr[$val['parent']]['sub']);
                $arr[$val['parent']]['sub'][$i] = $val;
                $arr[$val['id']]                = &$arr[$val['parent']]['sub'][$i];
            }
        }
        //print_r($result);exit;
        return $result;
    }

}
