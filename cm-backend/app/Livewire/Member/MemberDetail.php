<?php

namespace App\Livewire\Member;

use App\Enums\PublicEnum;
use App\Http\Controllers\Member\MemberController;
use App\Http\Controllers\Record\MainController;
use App\Http\Controllers\Record\TransactionLogController;
use Livewire\Component;
use Illuminate\Support\Facades\URL;
use App\Livewire\Common;

class MemberDetail extends Component
{
    public $type;

    public function mount()
    {
        if (null === session('memberId')) {
            $backRoute = URL::previousPath();
            redirect()->to($backRoute);
        }
        $this->getMemberData(session('memberId'));
        $this->getDropdown();
        $searchParams = $this->getSearchParams();
        $this->getTransactionLog($searchParams);
    }

    public function render()
    {
        return view('livewire.member.member-detail');
    }

    //搜尋
    public function doSearch(): void
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getTransactionLog($searchParams);
    }

    //分頁切換
    public function changePage($oldPage, $newPage): void
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getTransactionLog($searchParams);
        }
    }

    public function removeLogin($memberId)
    {
        if (!empty($memberId ?? '')) {
            $common = new Common();
            $requests = $common->getRequest(['member_id' => $memberId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $memberController = new MemberController();
            $result = $memberController->setTokenExpire($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $result->code) {
                $this->getMemberData(session('memberId'));
                session()->flash('pagePopMessage', __('web_message.memberDetail.removeLogin') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($result->message ?? [])->implode('; '));
            }
        }
    }

    public function unblocked($memberId)
    {
        if (!empty($memberId ?? '')) {
            $common = new Common();
            $requests = $common->getRequest(['member_id' => $memberId, 'status' => 1]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $memberController = new MemberController();
            $result = $memberController->updateStatus($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $result->code) {
                $this->getMemberData(session('memberId'));
                session()->flash('pagePopMessage', __('web_message.memberDetail.unblockedMember') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($result->message ?? [])->implode('; '));
            }
        }
    }

    public function blocked($memberId)
    {
        if (!empty($memberId ?? '')) {
            $common = new Common();
            $requests = $common->getRequest(['member_id' => $memberId, 'status' => 9]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $memberController = new MemberController();
            $result = $memberController->updateStatus($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $result->code) {
                $this->getMemberData(session('memberId'));
                session()->flash('pagePopMessage', __('web_message.memberDetail.blockedMember') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($result->message ?? [])->implode('; '));
            }
        }
    }

    public function passwordUnlock($memberId)
    {
        if (!empty($memberId ?? '')) {
            $common = new Common();
            $requests = $common->getRequest(['member_id' => $memberId, 'status' => 1, 'failCount' => 0]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $memberController = new MemberController();
            $result = $memberController->updatePasswrodLock($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $result->code) {
                $this->getMemberData(session('memberId'));
                session()->flash('pagePopMessage', __('web_message.memberDetail.passwordUnlock') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($result->message ?? [])->implode('; '));
            }
        }
    }

    public function resetGoogle2fa($memberId){
        if (!empty($memberId ?? '')) {
            $common = new Common();
            $requests = $common->getRequest(['member_id' => $memberId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $memberController = new MemberController();
            $result = $memberController->resetGoogle2fa($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $result->code) {
                $this->getMemberData(session('memberId'));
                session()->flash('pagePopMessage', __('web_message.memberDetail.resetGoogle2fa') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($result->message ?? [])->implode('; '));
            }
        }
    }

    public function doDepositList()
    {
        session()->flash('emitUsername', session()->get('pageCacheData')['memberData']->username ?? '');
        redirect()->to('/depositList');
    }

    public function doWithdrawalList()
    {
        session()->flash('emitUsername', session()->get('pageCacheData')['memberData']->username ?? '');
        redirect()->to('/withdrawalList');
    }

    private function getDropdown(): void
    {
        //接口
        $common = new Common();
        $dropDownRequests = $common->getRequest([]);
        if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
            $common->doLogout();
        }
        $mainController = new MainController();
        $typeResult = $mainController->getTypeDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['typeList'] = $typeResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    private function getMemberData($memberId): void
    {
        $common = new Common();
        $requests = $common->getRequest(['member_id' => $memberId]);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $memberController = new MemberController();
        $dataList = $memberController->view($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $dataList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['memberData'] = $dataList->data->member ?? [];
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($dataList->message ?? [])->implode('; '));
        }
    }

    //參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        $searchParams['member_id'] = session('memberId');
        if (-1 != ($this->type ?? -1)) {
            $searchParams['type'] = $this->type;
        }
        $searchParams['page'] = PublicEnum::DefaultPage;
        if (!empty(session('nowPage') ?? 0)) {
            $searchParams['page'] = session('nowPage');
        }
        $searchParams['per_page'] = PublicEnum::DefaultPageSize;
        return $searchParams;
    }

    private function getTransactionLog($params): void
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $transactionLogController = new TransactionLogController();
        $dataList = $transactionLogController->getList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $dataList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $dataList->data->list ?? [];
            $pageCacheData['totalPage'] = $dataList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $dataList->data->pagination->total ?? 0;
            session()->put('nowPage', $dataList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($dataList->message ?? [])->implode('; '));
        }
    }
}
