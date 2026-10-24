<?php
namespace App\Controllers\Agentadmin;

class CoreController extends BaseController
{
	public function actionCallAgentControllers(){

		$this->CheckPathPermissions();
		$path = $this->request->getAttribute('path');
        $params = $this->request->getParsedBody();
		$paths = explode('/', $path);//路由分发
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
		//处理
		$fun = isset($paths[2])?$paths[2]:'';
		if(isset($paths[2]) && strstr($paths[2], '_opt')){
			$fun = '';
			$funs = explode('_', $paths[2]);
			foreach ($funs as $key => $value) {
				$fun .= ucfirst($value);
			}
		}
		$actionFun = 'action'.$fun;
        $clsMethods = get_class_methods($controller);
        foreach ($clsMethods as $key => &$value) {
        	$value = strtolower($value);
        }
        if(!in_array(strtolower($actionFun),$clsMethods)){
            return $this->retJsonData($this->retMessage('error','不存在此方法'));
        }
		$result = $controller($actionFun,$params);
		return $this->retJsonData($result);
	}
}
