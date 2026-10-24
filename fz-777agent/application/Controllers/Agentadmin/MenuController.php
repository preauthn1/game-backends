<?php
namespace App\Controllers\Agentadmin;

use App\Module\ApiModule;
use App\Models\AgentMenuModel;

class MenuController extends BaseController
{

    //获取二级菜单
    public function actionGetAgentAdminSubMenu()
    {
        $params = $this->request->getQueryParams();
        
        $menus = [];
        if($this->session->get('agentTotalId')){
            $menus = AgentMenuModel::where('parent','=',$params['pid'])->where('type','=','0')->where('status','=','1')->orderBy('sort', 'asc')->get()->toArray();
        }
        if($this->session->get('agentId')){
            $menus = AgentMenuModel::where('parent','=',$params['pid'])->where('type','=','1')->where('status','=','1')->orderBy('sort', 'asc')->get()->toArray();
        }

        $ids = [];
        foreach ($menus as $key => $value) {
            $ids[] = $value['id'];
        }
        $subMenus = AgentMenuModel::whereIn('parent',$ids)->where('status','=','1')->orderBy('sort', 'asc')->get()->toArray();

        $result = [];
        if(empty($subMenus)){
            $result['name'] = $params['pname'];
            $result['children'] = $menus;
        }else{
            foreach ($menus as $key => $menu) {
                $data = [];
                $data['name'] = $menu['name'];
                foreach ($subMenus as $key => $subMenu) {
                    if($menu['id'] == $subMenu['parent']){
                        $d['id']        = 'sider-menu-'.$subMenu['id'];
                        $d['name']      = $subMenu['name'];
                        $d['target']    = 'navtab';
                        $d['url']       = $subMenu['url'].'?title='.$subMenu['name'];
                        $data['children'][] = $d;
                    }
                }
                if(empty($data['children'])){
                    $d['id']        = 'sider-menu-'.$menu['id'];
                    $d['name']      = $menu['name'];
                    $d['target']    = 'navtab';
                    $d['url']       = $menu['url'].'?title='.$menu['name'];
                    $data['children'][] = $d;
                }
                $result[] = $data;
            }
        }
        
        return json_encode($result);
    }

    public function actionApi()
    {
        $type = $this->request->getAttribute('type');
        $params = $this->request->getQueryParams();
        switch ($type) {
            case 'list':
                $result = $this->actionList();
                break;
            case 'editShow':
                $result = $this->actionEditShow();
                break;
            case 'edit':
                $result = $this->actionEdit();
                break;
            case 'del':
                $result = $this->actionDel();
                break;
            default:
                # code...
                break;
        }
        return ApiModule::retJsonData($result);
    }

    //获取所有菜单
    public function actionList()
    {
        $params = $this->request->getParsedBody();
        $pageCurrent = isset($params['pageCurrent'])?$params['pageCurrent']:1;
        $pageSize = isset($params['pageSize'])?$params['pageSize']:1000;

        $data = AgentMenuModel::orderBy('id', 'asc')->forPage($pageCurrent,$pageSize)->get()->toArray();
        foreach ($data as $key => &$value) {
            $value['typeName'] = $value['type']==1?'一级':'二级';
        }
        $counts = AgentMenuModel::count();

        $result['totalRow'] = $counts;
        $result['pageCurrent'] = $pageCurrent;
        $result['firstPage'] = true;
        $result['lastPage'] = false;
        $result['totalPage'] = round($counts/$pageSize);
        $result['pageSize'] = $pageSize;

        $result['list'] = $data;
        return $result;
    }

    public function actionEditShow()
    {
        $params = $this->request->getParsedBody();
        $menus = AgentMenuModel::orderBy('sort', 'asc')->get()->toArray();
        $thisViewArgs = [
            'title'   => '编辑信息',
            'menus'   => $menus,
            'params'  => $params
        ];
        return $this->render('qpadmin/menus/edit.html', $thisViewArgs);
    }

    public function actionEdit()
    {
        $params = $this->request->getParsedBody();
        if(empty($params['name']) || empty($params['ename']) || empty($params['parent'])){
            return ApiModule::retMessage('error','请求参数异常');
        }

        if(!empty($params['id'])){
            $data = AgentMenuModel::where('id',$params['id'])->update($params);    
        }else{
            $data = AgentMenuModel::create($params);
        }
        if(!$data){
            return ApiModule::retMessage('error','操作失败');
        }
        return $params;
        //return ApiModule::retMessage('ok','操作成功');
    }

    public function actionDel()
    {
         $params = $this->request->getParsedBody();
         if(empty($params['id'])){
            return ApiModule::retMessage('error','请求参数异常');
         }
         $data = AgentMenuModel::where('id',$params['id'])->delete(); 
         if(!$data){
            return ApiModule::retMessage('error','操作失败');
         }
         return ApiModule::retMessage('ok','操作成功');
    }
}
