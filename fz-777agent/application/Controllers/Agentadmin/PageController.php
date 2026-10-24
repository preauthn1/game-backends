<?php
namespace App\Controllers\Agentadmin;

use App\Models\AgentMenuModel;
use App\Module\BjuiGridModule;

class PageController extends BaseController
{
    protected $agentApiRoute = '/agentadmin/api/';
    protected $agentPageRoute = '/agentadmin/page/';

    //欢迎界面
    public function actionAgentAdminIndex()
    {
        $this->CheckPathPermissions();
        $page = $this->request->getAttribute('page');
        $params = $this->request->getParams();
        if(!empty($params) && strstr($page, '_opt')){//操作页面
            $params = !empty($params['id'])?$params:[];
            $agentApiRoute = $this->agentApiRoute.$page;
            $thisViewArgs = [
                'agentApiRoute'=>$agentApiRoute,
                'params' => $params,
            ];
            return $this->render("agentadmin/".$page.'.html', $thisViewArgs);
        }

        $agentInfo = $this->session->get('agentInfo');
        $agentTotalInfo = $this->session->get('agentTotalInfo');
        
        $logoutUrl = $agentAccount = $agentName = '';
        $topMenus = [];
        if($page=='main'){
            if(!empty($agentTotalInfo['id'])){
                $agentName = !empty($agentTotalInfo['username'])?$agentTotalInfo['username']:'';
                $logoutUrl = '/agentadmin/agenttotal/logout';
                $topMenus = AgentMenuModel::where('parent', 0)->where('type','=','0')->where('status','=','1')->orderBy('sort', 'asc')->get()->toArray();
            }
            if(!empty($agentInfo['id'])){
                $agentName = !empty($agentInfo['name'])?$agentInfo['name']:'';
                $logoutUrl = '/agentadmin/agent/logout';
                $topMenus = AgentMenuModel::where('parent', 0)->where('type','=','1')->where('status','=','1')->orderBy('sort', 'asc')->get()->toArray();
            }
        }
        //列表页面
        $listCfg = $this->getTemplatesListByPage($page);
        $initPageData = $this->initPageData($page,$params);
        $thisViewArgs = [
            'title'         => '棋牌后台管理中心',
            'pageHeader'    => '欢迎使用棋牌管理系统',
            'params'        => $params,
            'topMenus'      => $topMenus,
            'formSearch'    => $listCfg['formSearch'],
            'gridCfg'       => $listCfg['gridCfg'],
            'columnsData'   => $listCfg['columnsData'],
            'agentName'     => $agentName,
            'page'          => $page,
            'agentApiRoute' => $this->agentApiRoute.$page,
            'agentPageRoute'=> $this->agentPageRoute.$page,
            'logoutUrl'     => $logoutUrl,
            'initPageData'  => $initPageData,
            'agentInfo'      => $agentInfo,
            'agentTotalInfo' => $agentTotalInfo,
        ];
        return $this->render("agentadmin/".$listCfg['tpl'], $thisViewArgs);
    }

    public function initPageData($page,$params){
        if($page == 'login' || $page == 'main' || $page == 'agenttotal_login'){
            return [];
        }
        $paths = explode('/', $page);
        $controller = $this->getCfg('agentCtrl','agents.'.$paths[0].'.'.$paths[1]);
        if(empty($controller)){
            $contrs = explode('_', $paths[1]);
            $controller = '';
            foreach ($contrs as $key => $value) {
                $controller .= ucfirst($value);
            }
            $controller.='Controller';
        }
        $controller = 'App\Controllers\Agentadmin\\'.$controller;
        $controller = new $controller($this->container);
        $clsMethods = get_class_methods($controller);
        $actionFun = 'actionInitPageData';
        if(!in_array($actionFun,$clsMethods)){
            return [];
        }
        $result = $controller($actionFun,$params);
        return $result;
    }


    //根据路由获取模板
    public function getTemplatesListByPage($page){

        $formSearch = $columnsData = [];
        $gridCfg = '';
        //调用模板
        switch ($page) {
            case 'login':
                $page = "login.html";
                break;
            case 'agenttotal_login':
                $page = "agenttotal_login.html";
                break;
            case 'main':
                $page = "main.html";
                break;
            default:
                //读取配置
                $pageCfg = explode('/', $page);
                $buiFieldsCfg = $this->getCfg('BjuiFields',$pageCfg[0].'.'.$pageCfg[1]);
                $gridCfg = BjuiGridModule::getBjuiGridTplCfgObj($buiFieldsCfg['gridCfg']);
                $columnsData = isset($buiFieldsCfg['columns'])?$buiFieldsCfg['columns']:[];
                $columnsData = BjuiGridModule::getBjuiGridDataOptions($columnsData);
                $page = empty($buiFieldsCfg['tplCfg']['list'])?$page.'/list.html':"baselist.html";
                break;
        }
        $formSearch = '';
        if(!empty($buiFieldsCfg['formSearch'])){
            if(!empty($buiFieldsCfg['formSearch']['cols'])){
                $formSearch = BjuiGridModule::getAjaxSearchForm2($buiFieldsCfg['formSearch']);
            }else{
                $formSearch = BjuiGridModule::getAjaxSearchForm($buiFieldsCfg['formSearch']);
            }
        }
        //输出结果
        $result['formSearch'] = $formSearch;
        $result['tpl'] = $page;
        $result['gridCfg'] = $gridCfg;
        $result['columnsData'] = $columnsData;
        return $result;
    }
}
