<?php
namespace App\Controllers;

use App\Models\AgentMoneyUserModel;
use App\Models\BankExchangeModel;
use App\Models\GameBankModel;
use App\Models\GameExchangeModel;
use App\Models\ExchangeExportLogModel;
use App\Models\GamePlayerModel;
use App\Module\BjuiGridModule;
use App\Module\JqGridModule;

class ExportDataController extends BaseController
{

    public function actionCore()
    {
        $type = $this->request->getAttribute('type');
        switch ($type) {
            case 'exchangeBank':
                $this->actionExchangeBank();
                break;
            case 'passedExchangeBank':
                $this->actionPassedExchangeBank();
                break;
            case 'agentDetail':
                self::agentDetail();
                break;
            case 'agentDataDetail':
                self::agentDataDetail();
                break;
            case 'userList' :
                self::actionUserList();
                break;
            default:
                # code...
                break;
        }
    }

    public function actionUserList() {
        $this->baseAction();
        $colData = $this->getCfg('JqgridFields', 'userlist');
        if ($this->request->isGet()) {
            $param = $this->request->getParams();
            if (isset($param['page'])) unset($param['page']);
            $param['rows'] = 1000000;
            $list = GamePlayerModel::leftjoin('Money.Cash','Money.Cash.UserID','=','UserInfo.UserID')->leftjoin('Money.Bank','Money.Bank.UserID','=','UserInfo.UserID');
            $list = $list->selectRaw('UserInfo.UserID,Money.Cash.Amount,Money.Bank.Amount as bankgold,LoginName,UserType,FaceID,NickName,HDType,CreateTime,CreateIPAddress,LastLogonIPAddress,AreaID,SumCharge,SumAgentCharge,SumConvert,SumConvertTimes,SumTax,ISNULL(UserInfo.SumConvert, 0)+ISNULL(UserInfo.SumConverting, 0)+ISNULL(Money.Cash.Amount, 0)+ISNULL(Money.Bank.Amount, 0)-ISNULL(UserInfo.SumCharge, 0)-ISNULL(UserInfo.SumAgentCharge, 0) as TotalWin');
            $list = JqGridModule::jqGridsearchTrue($param,$list);
            $userTypeData = [1 => '正式账号',2 => '机器人',3 => '游客'];
            $hdTypeData = [1 => 'PC(1)', 2 => '安卓(2)', 3 => 'IOS(3)'];
            foreach ($list as $item) {
                if (!preg_match("/^1\d{10}$/", $item->LoginName)) {
                    $item->LoginName = "";
                }
                $item->UserType = $userTypeData[$item->UserType];
                $item->Amount = $item->Amount ? $item->Amount / 100 : 0;
                $item->bankgold = $item->bankgold ? $item->bankgold / 100 : 0;
                $item->SumCharge = $item->SumCharge ? $item->SumCharge / 100 : 0;
                $item->SumAgentCharge = $item->SumAgentCharge / 100;
                $item->SumConvert = $item->SumConvert / 100;
                $item->SumTax = $item->SumTax / 100;
                $item->TotalWin = $item->TotalWin / 100;
                $item->HDType = $hdTypeData[$item->HDType];
            }
            $this->exportData($list, $colData);
        }
    }

    //玩家兑换列表
    public function actionExchangeBank()
    {
        $this->baseAction();

        /*$colDatas = $this->getCfg('JqgridFields', 'exchangeBank');
        $colData = [
            'BankName' => $colDatas['BankName'],
            'BankAccountNumber' => $colDatas['BankAccountNumber'],
            'BankAccountName' => $colDatas['BankAccountName'],
            'Amount' => $colDatas['Amount'],
            'Id' => $colDatas['Id'],
        ];*/
        $exchangeStatus = GameExchangeModel::STATUS_DATA;
        $filePath = APP_ROOT.'/public/export_name.txt';
        if ($this->request->isGet()) {
            $param = $this->request->getParams();
            $list = BankExchangeModel::where("Status", 2);
            $list = JqGridModule::jqGridsearchTrue($param, $list)->toArray();
            $list = !empty($list['data'])?$list['data']:[];
            foreach ($list as $key => &$value) {
                $value['Status'] = $exchangeStatus[$value['Status']];
                $value['Amount'] = $value['Amount'] / 100;
                BankExchangeModel::where('Id', $value['Id'])->update(['IsExport' => 1]);
            }
            $data = [];
            foreach ($list as $k => $v) {
                $data[$k]['BankName'] = $v['BankName'];
                $data[$k]['BankAccountNumber'] = $v['BankAccountNumber'];
                $data[$k]['BankAccountName'] = $v['BankAccountName'];
                $data[$k]['Amount'] = $v['Amount'];
                $data[$k]['Id'] = $v['Id'];
            }
            $filename = file_exists($filePath) ? file_get_contents($filePath) : "";
            $filename = (!empty($filename) && ($filename>=date('Ymd001')))?($filename+1):date('Ymd001');
            file_put_contents($filePath, $filename);
            $this->exportExcelBank($data, $filename);
        }
    }

    //一键玩家银行兑换数据(非重复)
    public function actionPassedExchangeBank()
    {
        $this->baseAction();

        if ($this->request->isGet()) {
            $param = $this->request->getParams();
            $opt = $param['opt'];
            $export_time=$this->session->get("export_time");
            if($export_time){
                if($export_time+5>time()){
                    return ($opt==1)?$this->massage('操作太快！请稍候重试！', '/admin/exchangebank', 3):$this->massage('操作太快！请稍候重试！', '/admin/exchangexport', 3);
                }
            }
            $this->session->set("export_time",time());
            $filePath = APP_ROOT.'/export_name.txt';
            $filename = file_exists($filePath) ? file_get_contents($filePath) : date("Ymd001");
            $list = BankExchangeModel::where("Status", 2);
            $list = JqGridModule::jqGridsearchTrue($param, $list)->toArray();
            $list = !empty($list['data'])?$list['data']:[];
            $ids = [];
            $filename = (!empty($filename) && ($filename>=date('Ymd001')))?($filename+1):date('Ymd001');
            file_put_contents($filePath, $filename);
            $colData = $this->getCfg('JqgridFields', 'exchangeBank');
            $exchangeStatus = GameExchangeModel::STATUS_DATA;
            if(!empty($ids) && $filename){
                foreach ($list as $key => &$value) {
                    $value['Status'] = $exchangeStatus[$value['Status']];
                    BankExchangeModel::where('Id', $value['Id'])->update(['IsExport' => 1]);
                }
                $this->exportData($list, $colData);
            }else{

                return ($opt==1)?$this->massage('没有相关数据', '/admin/exchangebank', 3):$this->massage('没有相关数据', '/admin/exchangexport', 3);
            }
        }
    }
    //总代理，代理明细表导出
    private function agentDetail()
    {
        $this->baseAction();
        $data = $this->request->getParsedBody();
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentMoneyUserModel::selectRaw('agent_money_user.*,agent_user.name')->leftjoin('agent_user', 'agent_user.id', '=', 'agent_money_user.agent_id')->where('parent_id',$this->session->get('agentTotalId'));
        $list = BjuiGridModule::searchWhere($params,$list);
        $list = !empty($list['list'])?$list['list']:[];
        self::exportExcel($list, 'daili'.date("Ymd"));
    }
    //普通代理，代理交易明细表导出
    private function agentDataDetail()
    {
        $this->baseAction();
        $data = $this->request->getParsedBody();
        $params = BjuiGridModule::searchFormatterParam($data);
        $params['pageSize']= isset($data['pageSize'])?$data['pageSize']:'';
        $params['pageCurrent'] = isset($data['pageCurrent'])?$data['pageCurrent']:'';

        $list = AgentMoneyUserModel::where("agent_id",$this->session->get("agentId"))->select("id","user_id","money","type","new_money","status","remark","created_at","area");
        $list = BjuiGridModule::searchWhere($params,$list);
        $list = !empty($list['list'])?$list['list']:[];
        self::exportExcel($list, 'daili'.date("Ymd"));
    }

    /**
     * @param $expTableData
     * @throws \PHPExcel_Exception
     * @throws \PHPExcel_Reader_Exception
     * @throws \PHPExcel_Writer_Exception
     */
    function exportExcel($expTableData, $fileName) {
        require ANF_PATH . '/vendor/PHPExcel/Classes/PHPExcel.php';
        $objPHPExcel = new \PHPExcel();
        $objPHPExcel->getDefaultStyle()->getFont()->setName('宋体');
        $objPHPExcel->getActiveSheet()->fromArray($expTableData);
        $objPHPExcel->getActiveSheet()->getStyle('B')->getNumberFormat()->setFormatCode(\PHPExcel_Style_NumberFormat::FORMAT_TEXT);
        header('pragma:public');
        header('Content-type:application/vnd.ms-excel;charset=utf-8;name="' . $fileName . '.xls"');
        header("Content-Disposition:attachment;filename=$fileName.xls"); //attachment新窗口打印inline本窗口打印
        $objWriter = \PHPExcel_IOFactory::createWriter($objPHPExcel, 'Excel5');
        $objWriter->save('php://output');
        exit;
    }
    
    function exportExcelBank($expTableData, $fileName) {
        require ANF_PATH . '/vendor/PHPExcel/Classes/PHPExcel.php';
        $objPHPExcel = new \PHPExcel();
        try{
            $objPHPExcel->getDefaultStyle()->getFont()->setName( '宋体');
            foreach ($expTableData as $k => $v){
                $objPHPExcel->getActiveSheet()->setCellValue('A'.($k+1),$v["BankName"])->setCellValueExplicit('B'.($k+1),$v["BankAccountNumber"],\PHPExcel_Cell_DataType::TYPE_STRING)
                    ->setCellValue('C'.($k+1),$v["BankAccountName"])->setCellValue('D'.($k+1),$v["Amount"])->setCellValue('E'.($k+1),$v["Id"]);
                $objPHPExcel->getActiveSheet()->getStyle('B'.($k+1))->getNumberFormat()->setFormatCode(\PHPExcel_Style_NumberFormat::FORMAT_TEXT);
            }
            header('pragma:public');
            header('Content-type:application/vnd.ms-excel;charset=utf-8;name="' . $fileName . '.xls"');
            header("Content-Disposition:attachment;filename=$fileName.xls");
            $objWriter = \PHPExcel_IOFactory::createWriter($objPHPExcel, 'Excel5');
            $objWriter->save('php://output');
        }catch (\PHPExcel_Writer_Exception $e){}
        catch (\PHPExcel_Reader_Exception $e){}
        catch (\PHPExcel_Exception $e){}
        exit;
    }
}
