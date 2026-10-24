<?php
namespace App\Module;

use Illuminate\Pagination\Paginator;
use App\Controllers\Agentadmin\SelectController;

/**
 *  JqGrid公共模块
 */
class BjuiGridModule
{

    /**
    *@desc 获取指定参数
    */
    public static function addSearchParamValue($data,$param=[]){
        if(!empty($data['_search']) && !empty($param)){
            foreach ($param as $key => $value) {
                array_push($data['_search'],['field'=>$value[0],'op'=>$value[1],'data'=>$value[2]]);
            }
        }
        return $data;
    }

    /**
    *@desc 获取指定参数值
    */
    public static function getSearchParamValue($data,$param=''){
        if(!empty($data['_search']) && !empty($param)){
            foreach ($data['_search'] as $key => &$value) {
                if($value['field'] == $param){
                    return $value['data'];
                }
            }
        }
        return '';
    }

    /**
     * API返回json数据格式
     * @param array $data 数据
     * @return string
     */
    public static function retJsonData($data=[]){
        $jsoncallback = !empty($_REQUEST['callback'])?htmlspecialchars($_REQUEST['callback']):'';
        return $jsoncallback ? $jsoncallback . "(" . json_encode($data) . ")" : json_encode($data);  
        exit;  
    }

    /**
     * API返回json数据格式
     * @param array $data 数据
     * @return string
     */
    public static function retMessage($status='',$message="")
    {
        $statusCode = ($status=='ok')?200:300;
        return array('statusCode'=>$statusCode,'message'=>$message);
    }

    /**
     * 格式化搜索格式
     * @param array $params 参数
     * @return array
     */
    public static function searchFormatterParam($params){
        $data = [];
        $params = !empty($params)?$params:[];
        if(isset($params['searchObj'])){
            foreach ($params['searchObj'] as $key => $value) {
                if(strstr($key, '|')){
                    $keys = explode('|', $key);
                    $result['_search'][] = ['field'=>$keys[0],'op'=>strtolower($keys[1]),'data'=>$value];
                }else{
                    if(count(explode(' - ', $value)) == 2){
                        $value = explode(' - ', $value);
                        $result['_search'][] = ['field'=>$key,'op'=>'>=','data'=>$value[0]];
                        $result['_search'][] = ['field'=>$key,'op'=>'<=','data'=>$value[1]];
                    }else{
                        $result['_search'][] = ['field'=>$key,'op'=>'=','data'=>$value];
                    }
                }
            }
        }else{
            foreach ($params as $key => $value) {
                if(!strstr($key, '_operator') && strstr($key, 'obj_')){
                    $data[$key] = $value;
                }
            }
            $result = [];
            foreach ($data as $key => $value) {
                $result['_search'][] = ['field'=>str_replace('obj_', '', $key),'op'=>strtolower($params[$key.'_operator']),'data'=>$value];
            }
        }
        if(!empty($params['orders'])){
            $result['orders'] = $params['orders'];
        }
        return $result;
    }

    //Bjuigrid条件搜索
    public static function searchWhere($params,$list,$groupby=[]){
        $pageSize = isset($params['pageSize'])?$params['pageSize']:'';
        $pageCurrent = isset($params['pageCurrent'])?$params['pageCurrent']:'';
        $ops = ['=','<>','<','<=','>','>=','like','not like'];
        if(!empty($params['_search'])) {
            foreach ($params['_search'] as $key=>$val) {
                if(in_array($val['op'], $ops) && $val['data']!=''){
                    switch ($val['op']) {
                        case 'like':
                            $list->where($val['field'], $val['op'], '%' . $val['data'] . '%');
                            break;
                        case 'not like':
                            $list->where($val['field'], $val['op'], '%' . $val['data'] . '%');
                            break;
                        case "in":
                            $val['data'] = explode(',', $val['data']);
                            $list->whereIn($val['field'], $val['data']);
                        case "not in":
                            $val['data'] = explode(',', $val['data']);
                            $list->whereIn($val['field'], $val['data']);
                        default:
                            $list->where($val['field'], $val['op'], $val['data']);
                            break;
                    }
                }
            }
        }

        $counts = $list->count();

        if(!empty($groupby)){
            $list=$list->groupBy($groupby);
        }

        if(!empty($params['orders'])){//排序操作
           $orderBys = explode(',', $params['orders']);
           foreach ($orderBys as $key => $value) {
               $orders = explode(' ', $value);
               $list->orderBy($orders[0], $orders[1]);
           }
        }else{
            $list->orderBy('id', 'desc');
        }
        
        if(!empty($pageSize) && !empty($pageCurrent)){
            $list=$list->forPage($pageCurrent,$pageSize);
        }

        $lists = $list->get()->toArray();

        $result['totalRow'] = $counts;
        $result['pageCurrent'] = $pageCurrent;
        $result['firstPage'] = true;
        $result['lastPage'] = false;
        $result['totalPage'] = !empty($pageSize)?round($counts/$pageSize):0;
        $result['pageSize'] = $pageSize;
        $result['list'] = $lists;

        return $result;
    }

    public static function getBjuiGridTplCfgObj($config=[]){
        $tplObj = '{
            "tableWidth": "100%",
            "height": "100%",
            "gridTitle": "标题",
            "showToolbar": false,
            "toolbarItem": "add,edit,del,|,import,export,exportf",
            "toolbarCustom":"",
            "showEditbtnscol":false,
            "dataUrl": "",
            "dataType": "jsonp",
            "jsonPrefix": "obj",
            "editMode": {"dialog": {"width": 800,"height": 500,"title": "编辑信息","mask": "true"}},
            "editUrl": "edit.html?code={code}",
            "delUrl":"",
            "delType":"POST",
            "delPK":"id",
            "paging": {"pageSize": 15},
            "showCheckboxcol":false,
            "linenumberAll":true,
            "columnMenu":true,
            "filterThead":false,
            "columnFilter":true,
            "importOption":{"type": "dialog","options": {"url": "","width": 500,"height": 300,"title": "导入标题"}},
            "exportOption":{"type": "file","options": {"url": "","loadingmask": true}}
        }';

        if(!empty($config)){
            $tplObj = json_decode($tplObj,true);
            foreach ($tplObj as $key => &$value) {
                $value = isset($config[$key])?$config[$key]:$value;
            }
            $tplObj = json_encode($tplObj);
        }
        return $tplObj;
    }

    public static function getBjuiGridDataOptions($columns){
        $dfColumns = ['name'=>'','type'=>'text','align'=>'center'];
        foreach ($columns as $key => &$value) {
            if(empty($value['dataOptions'])){
                $value['dataOptions'] = ['name'=>$key,'type'=>'text','align'=>'center'];
            }else{
                $value['dataOptions'] = array_merge($dfColumns,$value['dataOptions']);
            }
            if(!empty($value['dataOptions']['render'])){
                $value['dataOptions']['render'] = self::$value['dataOptions']['render']();
            }
            $value['dataOptions'] = self::jsonEncode($value['dataOptions']);
        }
        return $columns;
    }

    public static function getAjaxSearchForm($searchforms)
    {
        $form = '<form class="ajaxSerachForm" data-toggle="ajaxsearch" data-options="{searchDatagrid:$.CurrentNavtab.find(\'#datagrid-test-filter\')}"><fieldset>';
        $form .= '<legend style="font-weight:normal;">搜索：</legend>';
        $form .= '<div style="margin:0; padding:1px 5px 5px;">';
        if(!empty($searchforms)){
            foreach ($searchforms as $key => $value) {
                $spanTitle = '<span>'.$value['name'].'：';
                switch ($value['type']) {
                    case 'input':
                        $form .= $spanTitle.'<input type="text" name="searchObj['.$key.']" class="form-control" size="'.(!empty($value['size'])?$value['size']:15).'">&nbsp;&nbsp;&nbsp;&nbsp;'.(!empty($value['newline'])?'<br/><br/>':'');
                        break;
                    case 'inputrange':
                        $form .= $spanTitle.'<input type="text" name="searchObj['.$key.'|>=]" class="form-control" size="'.(!empty($value['size'])?$value['size']:15).'">&nbsp;'.(isset($value['sign'])?$value['sign']:'-');
                        $form .= '&nbsp;<input type="text" name="searchObj['.$key.'|<=]" class="form-control" size="'.(!empty($value['size'])?$value['size']:15).'">&nbsp;&nbsp;&nbsp;&nbsp;'.(!empty($value['newline'])?'<br/><br/>':'');
                        break;
                    case 'datetimerange':
                        $form .= $spanTitle.'<input type="text" data-toggle="daterangepicker" style="'.(isset($value['style'])?$value['style']:'').'" class="form-control datetimestamp-daterangepicker" name="searchObj['.$key.']" id="'.$key.'" size="'.$value['size'].'">&nbsp;&nbsp;&nbsp;&nbsp;'.(!empty($value['newline'])?'<br/><br/>':'');
                        break;
                    case 'date':
                        $form .= $spanTitle.'<input type="text" data-toggle="datepicker" class="form-control" name="searchObj['.$key.']" id="'.$key.'" size="'.(!empty($value['size'])?$value['size']:15).'">&nbsp;&nbsp;&nbsp;&nbsp;'.(!empty($value['newline'])?'<br/><br/>':'');
                        break; 
                    case 'select':
                        $option = (isset($value['selectAll']) && $value['selectAll'] == 'none')?'':'<option value="">全部</optiop>';
                        if(!empty($value['option'])){
                            if(!is_array($value['option'])){
                                $ctrClsName = 'action'.$value['option'];
                                $value['option'] = SelectController::$ctrClsName();
                            }
                            foreach ($value['option'] as $k => $v) {
                                $option .= (isset($value['select']) && $value['select']=="$k")?'<option value="'.$k.'"  selected>'.$v.'</optiop>':'<option value="'.$k.'">'.$v.'</optiop>';
                            }
                        }
                        $form .= $spanTitle.'<select data-toggle="selectpicker" data-width="'.(isset($value['data-width'])?$value['data-width']:'').'" name="searchObj['.$key.']" id="'.$key.'">'.$option.'</select>&nbsp;&nbsp;&nbsp;&nbsp;'.(!empty($value['newline'])?'<br/><br/>':'');
                        break;
                    default:
                        # code...
                        break;
                }
            }
        }
        $form .= '<div class="btn-group"><button type="submit" class="btn-green" data-icon="search">开始搜索！</button><button type="reset" class="btn-orange" data-icon="times">重置</button></div></div></fieldset></form>';
        return $form;
    }

    public static function getAjaxSearchForm2($searchforms)
    {
        $cols = !empty($searchforms['cols'])?$searchforms['cols']:3;
        $form = '<form data-toggle="ajaxsearch" data-options="{searchDatagrid:$.CurrentNavtab.find(\'#datagrid-test-filter\')}"><fieldset>';
        $form .= '<legend style="font-weight:normal;">页头搜索：</legend>';
        $form .= '<div class="bjui-row col-'.$cols.'">';
        if(!empty($searchforms['dataOptions'])){
            foreach ($searchforms['dataOptions'] as $key => $value) {
                switch ($value['type']) {
                    case 'button':
                        $form .= '<label class="row-label">'.$value['name'].'：</label><div class="row-input"><button type="button" name="searchObj['.$key.']" class="btn-default">普通按钮</button></div>';
                    case 'input':
                        $form .= '<label class="row-label">'.$value['name'].'：</label><div class="row-input"><input type="text" name="searchObj['.$key.']" class="form-control" size="'.(!empty($value['size'])?$value['size']:20).'"></div>';
                        break;
                    case 'datetimerange':
                        $form .= '<label class="row-label">'.$value['name'].'：</label><div class="row-input"><input type="text" data-toggle="daterangepicker" style="'.(isset($value['style'])?$value['style']:'').'" class="form-control datetimestamp-daterangepicker" name="searchObj['.$key.']" id="'.$key.'" size="'.$value['size'].'"></div>';
                        break;
                    case 'date':
                        $form .= '<label class="row-label">'.$value['name'].'：</label><div class="row-input"><input type="text" data-toggle="datepicker" class="form-control" name="searchObj['.$key.']" id="'.$key.'" size="'.$value['size'].'"></div>';
                        break; 
                    case 'select':
                        $option = (isset($value['selectAll']) && $value['selectAll'] == 'none')?'':'<option value="">全部</optiop>';
                        if(!empty($value['option'])){
                            if(!is_array($value['option'])){
                                $ctrClsName = 'action'.$value['option'];
                                $value['option'] = SelectController::$ctrClsName();
                            }
                            foreach ($value['option'] as $k => $v) {
                                $option .= (isset($value['select']) && $value['select']=="$k")?'<option value="'.$k.'"  selected>'.$v.'</optiop>':'<option value="'.$k.'">'.$v.'</optiop>';
                            }
                        }
                        $form .= '<label class="row-label">'.$value['name'].'：</label><div class="row-input"><select data-toggle="selectpicker" data-width="'.(!empty($value['data-width'])?$value['data-width']:20).'" name="searchObj['.$key.']" id="'.$key.'">'.$option.'</select></div>';
                        break;
                    default:
                        # code...
                        break;
                }
            }
        }
        $form .= '<div class="btn-group"><button type="submit" class="btn-green" data-icon="search">开始搜索！</button><button type="reset" class="btn-orange" data-icon="times">重置</button></div></div></fieldset></form>';
        return $form;
    }

    //格式化回调函数
    public static function jsonEncode($colModel){
        $colStr=json_encode($colModel);
        $colStr=preg_replace("/render\":\"(.*?)\"/is", "render\":\\1", $colStr);
        $colStr=preg_replace('/render\':\'(.*?)\'/is', 'render\':\\1', $colStr);
        return $colStr;
    }

    /**
    *@desc 根据用户ID跳转到用户详细页面方法
    */
    public static function getRenderUrl($url=''){
        return 'function(value,data){return \'<a href="/admin/user/info/\'+value+\'">\'+value+\'</a>\'}';
    }
}