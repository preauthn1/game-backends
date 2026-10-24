<?php
namespace App\Module;

use Illuminate\Pagination\Paginator;

/**
 *  JqGrid公共模块
 */
class JqGridModule
{

    /**
     *@desc 获取指定参数
     */
    public static function getFiltersRuleParam($data,$param=[]){
        if(!empty($data['filters']) && !empty($param)){
            $groupOp = (array)json_decode($data['filters']);
            foreach ($groupOp['rules'] as $key => &$value) {
                if(!in_array($value->field, $param)){
                    unset($groupOp['rules'][$key]);
                }
            }
            $data['filters'] = json_encode($groupOp);
            return $data;
        }
        return $data;
    }

    /**
     *@desc 获取指定参数值
     */
    public static function getFiltersRuleParamValue($data,$param=''){
        if(!empty($data['filters']) && !empty($param)){
            $groupOp = (array)json_decode($data['filters']);
            foreach ($groupOp['rules'] as $key => &$value) {
                if($value->field == $param){
                    return $value->data;
                }
            }
        }
        return '';
    }


    /**
     *@desc 获取指定参数
     */
    public static function filtersRuleParam($data,$param=[]){
        if(!empty($data['filters']) && !empty($param)){
            $groupOp = (array)json_decode($data['filters']);
            foreach ($groupOp['rules'] as $key => &$value) {
                if(in_array($value->field, $param)){
                    unset($groupOp['rules'][$key]);
                }
            }
            $data['filters'] = json_encode($groupOp);
            return $data;
        }
        return $data;
    }

    /**
     *@desc 默认当天数据
     */
    public static function getFiltersTodayParam($data,$field='created_at'){
        $startdate = date('Y-m-d 00:00:00');
        $endate = date('Y-m-d 00:00:00', strtotime('+1 day'));
        if (empty($data['filters']) || empty(json_decode($data['filters'])->rules)) {//默认当天数据
            $data['filters'] = json_encode(['groupOp' => 'AND', 'rules' => [['field' => $field, 'op' => 'ge', 'data' => $startdate], ['field' => $field, 'op' => 'le', 'data' => $endate]]]);
            $data['_search'] = true;
        } else {
            $time_start = JqGridModule::getFiltersRuleParamValue($data, $field);
            if (empty($time_start)) {
                $data['filters'] = json_decode($data['filters'], true);
                $data['filters']['rules'] = array_merge($data['filters']['rules'], [['field' => $field, 'op' => 'ge', 'data' => $startdate], ['field' => $field, 'op' => 'le', 'data' => $endate]]);
                $data['filters'] = json_encode($data['filters']);
            }
        }
        return $data;
    }

    /**
    *@desc 根据用户ID跳转到用户详细页面方法
    */
    public static function getFormatterPlatform(){
        return 'function renderPlatform(cellvalue, options, rowObject){return cellvalue==1?"ISO(1)":"安卓(0)"}';
    }

    /**
     * HDType方法
     * @return string
     */
    public static function getFormaterHDType() {
        return 'function renderHDType(cellvalue, options, rowObject){ if(cellvalue == 0) return "机器人(0)"; else if(cellvalue == 1) return "PC(1)";else if(cellvalue == 2) return "安卓(2)"; else if(cellvalue == 3) return "IOS(3)";else return "";}';
    }

    /**
    *@desc 根据用户ID跳转到用户详细页面方法
    */
    public static function getFormatterUserInfo(){
        return 'function renderUserInfo(cellvalue, options, rowObject){return \'<a href="/admin/user/info/\'+cellvalue+\'" target="_blank">\'+cellvalue+\'</a>\'}';
    }

    public static function getAjaxFun($str){
        return "if(!sessionStorage.$str){
                    $.ajax({  
                        type: 'post',url: '',dataType: 'text',async:false,data: 'oper=$str',  
                        success: function(msg){
                            sessionStorage.$str=msg;    
                        }
                    });
                }
                var $str=$.parseJSON(sessionStorage.$str);";
    }

    //自定义方法
    public static function getCustomFormatter($fun,$data){
        return 'function '.$fun.'(cellvalue, options, rowObject){
                    var data='.json_encode($data).';
                    if(!cellvalue && cellvalue!=0){
                        return "";
                    }else{
                        return data[cellvalue]+" ("+cellvalue+")";
                    }
                }';
    }

    public static function getTrueOrFalseFormatter($fun, $data) {
        return 'function '.$fun.'(cellvalue, options, rowObject){
                    return cellvalue == 0 ? "否" : "是";
                }';
    }

    public static function getFormatter($fun,$data){
        return 'function '.$fun.'(cellvalue, options, rowObject){
                    if(!cellvalue&&cellvalue!=0){
                        return "";
                    }else{
                        return '.$data.'[cellvalue]+"("+cellvalue+")";
                    }
                } function goldToYuan(cellvalue, options, rowObject){return cellvalue/100;}';
    }

    public static function getFormatterColor($fun,$color,$name){
        return 'function '.$fun.'(cellvalue, options, rowObject){
            var color='.json_encode($color).';
            var name='.json_encode($name).';
            return \'<span class="label label-lg label-\'+color[cellvalue]+\'">\'+name[cellvalue]+"("+cellvalue+")</span>";}';
    }

    public static function getFormatterUnequal($fun){
        return 'function '.$fun.'(cellvalue, options, rowObject){
            cellvalue = cellvalue/100;
            if(cellvalue>0){
                return \'<span class="label label-lg label-success">\'+cellvalue+\'</span>\';
            }else if(cellvalue<0){
                return \'<span class="label label-lg label-danger">\'+cellvalue+\'</span>\';
            }
            return cellvalue;}';
    }


    public static function getNullFormatter(){
        return 'function nullFormatter(cellvalue, options, rowObject){if (cellvalue === undefined || cellvalue === \'null\' || cellvalue === \'NULL\' || cellvalue === null) {cellvalue = \'\';}return cellvalue;}';
    }

    public static function getObj(){
        return (object) array('sortable' => true,'editable'=>false,'width'=>20,'sorttype'=>'int','name'=>'title','formatter'=>self::getNullFormatter());
    }

    public static function jsonEncode($colModel){
        $colStr=json_encode($colModel);
        $colStr=preg_replace("/formatter\":\"(.*?)\",/is", "formatter\":\\1,", $colStr);
        $colStr=preg_replace("/formatoptions\":\"(.*?)\",/is", "formatoptions\":\\1,", $colStr);
        return $colStr;
    }

    //获取JqGrid格式化数据
    public static function getsJqGridColData($colData){
        $colNames = [];
        $colModel = [];
        foreach ($colData as $key => $value) {
            $colNames[] = $value['name'];
            $obj = self::getObj();
            $obj->name = $key;
            $obj->index = $key;
            $obj->sorttype = (isset($value['sorttype']))?$value['sorttype']:$obj->sorttype;
            $obj->formatter = (isset($value['formatter']))?$value['formatter']:$obj->formatter;
            $obj->width = (isset($value['width']))?$value['width']:$obj->width;
            $obj->sortable = (isset($value['sortable']))?$value['sortable']:$obj->sortable;
            array_push($colModel,$obj);
        }
        return ['colModel'=>self::jsonEncode($colModel),'colNames'=>json_encode($colNames)];
    }

    public static function getsJqGridSearch($searchforms)
    {
        $form = '';
        if(!empty($searchforms)){
            $form = '<div style="margin:2.5px;margin-right:1%;float:right"><form class="form-inline search-forms">';
            foreach ($searchforms as $key => $value) {
                switch ($value['type']) {
                    case 'input':
                        $val = isset($value['value']) ? $value['value'] : "";
                        $form .= '<div class="form-group"><label for="'.$value['index'].'">'.$value['name'].' : </label> <input type="text" style="'.(isset($value['style'])?$value['style']:'').'" class="form-control" value="'.$val.'" name="'.$value['index'].'" id="'.$value['index'].'"  placeholder="'.(isset($value['placeholder'])?$value['placeholder']:$value['name']).'"></div> ';
                        break;
                    case 'datetimestamp':
                        $form .= '<div class="form-group"><label for="'.$value['index'].'">'.$value['name'].' : </label> <input type="text" style="'.(isset($value['style'])?$value['style']:'').'" class="form-control datetimestamp-daterangepicker" name="'.$value['index'].'" id="'.$value['index'].'"  placeholder="'.(isset($value['placeholder'])?$value['placeholder']:$value['name']).'"></div> ';
                        break;
                    case 'date':
                        $form .= '<div class="form-group"><label for="'.$value['index'].'">'.$value['name'].' : </label> <input type="text" style="'.(isset($value['style'])?$value['style']:'').'" class="form-control date-daterangepicker" name="'.$value['index'].'" id="'.$value['index'].'"  placeholder="'.(isset($value['placeholder'])?$value['placeholder']:$value['name']).'"></div> ';
                        break; 
                    case 'select':
                        $option='';
                        if($value['option']){
                            $option = (isset($value['selectAll']) && $value['selectAll'] == 'none')?'':'<option value="">全部</optiop>';
                            foreach ($value['option'] as $k => $v) {
                                $option .= (isset($value['select']) && $value['select']=="$k")?'<option value="'.$k.'"  selected>'.$v.'</optiop>':'<option value="'.$k.'">'.$v.'</optiop>';
                            }
                        }
                        $id =  isset($value['id']) ? $value['id'] : $value['index'];
                        $form .= '<div class="form-group"><label for="'.$value['index'].'">'.$value['name'].' : </label> <select style="'.(isset($value['style'])?$value['style']:'').'" class="form-control" name="'.$value['index'].'" id="'.$id.'"  placeholder="'.(isset($value['placeholder'])?$value['placeholder']:$value['name']).'">'.$option.'</select></div> ';
                        break;
                    default:
                        # code...
                        break;
                }
            }
            $form .= '<input id="search" class="btn btn-purple btn-sm" type="button" onclick="addJqGridFormSearch()" value="查  询" /></form></div>';
        }
        return $form;
    }


    //jqgrid搜索
    public static function jqGridsearchTrue($req,$list,$groupby=[],$simple=false){
        Paginator::currentPageResolver(function ($pageName) {
            return empty($_REQUEST[$pageName]) ? 1 : $_REQUEST[$pageName];
        });
        if($req['_search']=="true") {
            if(isset($req['filters'])) {
                $filters = json_decode($req['filters']);
                $rules = $filters->rules;
                $groupOp = $filters->groupOp;
                if ($groupOp == "OR") {
                    foreach ($rules as $val) {
                        switch ($val->op) {
                            case "eq":
                                $list->orWhere($val->field, $val->data);
                                break;
                            case "ne":
                                $list->orWhere($val->field, "<>", $val->data);
                                break;
                            case "lt":
                                $list->orWhere($val->field, "<", $val->data);
                                break;
                            case "le":
                                $list->orWhere($val->field, "<=", $val->data);
                                break;
                            case "gt":
                                $list->orWhere($val->field, ">", $val->data);
                                break;
                            case "ge":
                                $list->orWhere($val->field, ">=", $val->data);
                                break;
                            case "cn":
                                $list->orWhere($val->field, "like", '%' . $val->data . '%');
                                break;
                            case "nc":
                                $list->orWhere($val->field, "not like", '%' . $val->data . '%');
                                break;
                            case "in":
                                $val->data = explode(',', $val->data);
                                $list->orWhereIn($val->field, $val->data);
                                break;
                        }
                    }
                } else {
                    foreach ($rules as $val) {
                        switch ($val->op) {
                            case "eq":
                                $list->where($val->field, $val->data);
                                break;
                            case "ne":
                                $list->where($val->field, "<>", $val->data);
                                break;
                            case "lt":
                                $list->where($val->field, "<", $val->data);
                                break;
                            case "le":
                                $list->where($val->field, "<=", $val->data);
                                break;
                            case "gt":
                                $list->where($val->field, ">", $val->data);
                                break;
                            case "ge":
                                $list->where($val->field, ">=", $val->data);
                                break;
                            case "cn":
                                $list->where($val->field, "like", '%' . $val->data . '%');
                                break;
                            case "nc":
                                $list->where($val->field, "not like", '%' . $val->data . '%');
                                break;
                            case "in":
                                $val->data = explode(',', $val->data);
                                $list->whereIn($val->field, $val->data);
                                break;
                        }
                    }
                }
            }else{
                $filter=$req;unset($filter['_search']);unset($filter['nd']);unset($filter['rows']);unset($filter['page']);unset($filter['sidx']);unset($filter['sord']);
                foreach ($filter as $k => $v){
                    $list->where($k, $v);
                }
            }
        }

       if(!empty($groupby)){
            $list=$list->groupBy($groupby);
        }

        if(empty($req['sidx'])){
            if($simple){
                $list=$list->simplePaginate($req['rows']);
            }else{
                $list=$list->paginate($req['rows']);
            }
        }else{
            if($simple){
                $list=$list->orderByRaw($req['sidx'].' '.$req['sord'])->simplePaginate($req['rows']);
            }else{
                $list=$list->orderByRaw($req['sidx'].' '.$req['sord'])->paginate($req['rows']);
            }
        }
        return $list;
    }

    //不带分页jqgrid条件搜索
    public static function jqGridSelectWhere($param, $list, $groupby=[]) {
        $filters = !empty($param['filters']) ? json_decode($param['filters']) : [];
        $types = ['eq'=>'=','ne'=>'<>','lt'=>'<','le'=>'<=','gt'=>'>','ge'=>'>=','cn'=>'like','nc'=>'not like'];
        if(!empty($filters->rules)){
            if (!empty($filters->groupOp) && $filters->groupOp == "OR") {
                foreach ($filters->rules as $val) {
                    if(in_array($val->op, ['like','not linke'])){
                        $list->orWhere($val->field, "{$types[$val->op]}", '%' . $val->data . '%');
                    }else{
                        $list->orWhere($val->field, "{$types[$val->op]}", $val->data); 
                    }
                }
            }else{
                foreach ($filters->rules as $val) {
                    if(in_array($val->op, ['like','not linke'])){
                        $list->where($val->field, "{$types[$val->op]}", '%' . $val->data . '%');
                    }else{
                        $list->where($val->field, "{$types[$val->op]}", $val->data); 
                    }
                }
            }
        }

        if(!empty($groupby)){
            $list=$list->groupBy($groupby);
        }

        if (empty($param['sidx'])) {
            $list = $list->get();
        } else {
            $list = $list->orderBy($param['sidx'], $param['sord'])->get();
        }
        return $list;
    }

    //jqgrid搜索建立
    public static function jqGridsearchBuild($req,$list,$groupby=[]){
        if($req['_search']=="true") {
            if(isset($req['filters'])) {
                $filters = json_decode($req['filters']);
                $rules = $filters->rules;
                $groupOp = $filters->groupOp;
                if ($groupOp == "OR") {
                    foreach ($rules as $val) {
                        switch ($val->op) {
                            case "eq":
                                $list->orWhere($val->field, $val->data);
                                break;
                            case "ne":
                                $list->orWhere($val->field, "<>", $val->data);
                                break;
                            case "lt":
                                $list->orWhere($val->field, "<", $val->data);
                                break;
                            case "le":
                                $list->orWhere($val->field, "<=", $val->data);
                                break;
                            case "gt":
                                $list->orWhere($val->field, ">", $val->data);
                                break;
                            case "ge":
                                $list->orWhere($val->field, ">=", $val->data);
                                break;
                            case "cn":
                                $list->orWhere($val->field, "like", '%' . $val->data . '%');
                                break;
                            case "nc":
                                $list->orWhere($val->field, "not like", '%' . $val->data . '%');
                                break;
                            case "in":
                                $val->data = explode(',', $val->data);
                                $list->orWhereIn($val->field, $val->data);
                                break;
                        }
                    }
                } else {
                    foreach ($rules as $val) {
                        switch ($val->op) {
                            case "eq":
                                $list->where($val->field, $val->data);
                                break;
                            case "ne":
                                $list->where($val->field, "<>", $val->data);
                                break;
                            case "lt":
                                $list->where($val->field, "<", $val->data);
                                break;
                            case "le":
                                $list->where($val->field, "<=", $val->data);
                                break;
                            case "gt":
                                $list->where($val->field, ">", $val->data);
                                break;
                            case "ge":
                                $list->where($val->field, ">=", $val->data);
                                break;
                            case "cn":
                                $list->where($val->field, "like", '%' . $val->data . '%');
                                break;
                            case "nc":
                                $list->where($val->field, "not like", '%' . $val->data . '%');
                                break;
                            case "in":
                                $val->data = explode(',', $val->data);
                                $list->whereIn($val->field, $val->data);
                                break;
                        }
                    }
                }
            }else{
                $filter=$req;unset($filter['_search']);unset($filter['nd']);unset($filter['rows']);unset($filter['page']);unset($filter['sidx']);unset($filter['sord']);
                foreach ($filter as $k => $v){
                    $list->where($k, $v);
                }
            }
        }

        if(!empty($groupby)){
            $list=$list->groupBy($groupby);
        }

        if(!empty($req['sidx'])){
            $list=$list->orderBy($req['sidx'], $req['sord']);
        }
        return $list;
    }
}