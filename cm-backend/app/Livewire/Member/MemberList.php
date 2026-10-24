<?php

namespace App\Livewire\Member;

use App\Enums\PublicEnum;
use Livewire\Component;
use App\Http\Controllers\Member\MemberController;
use App\Livewire\Common;
use App\Http\Controllers\Withdrawal\BankController;
use App\Http\Controllers\Country\MainController;
use Maatwebsite\Excel\Facades\Excel;
use App\Exports\GenericExport;

class MemberList extends Component
{
    //搜尋
    public $username, $fuzzy = false, $uid, $agentName, $realName, $countryId, $mobileNo, $referralCode, $registerIp, $lastLoginIp, $memberType, $registerRange, $accountNo, $cryptoAddr, $memberStatus;
    //監聽註冊區間搜尋
    protected $listeners = ['registerRange' => 'modifyRegisterRange'];
    //新增
    public $addMemberType = 2, $addMemberUsername, $addMemberPassword, $addMemberFundPassword, $addMemberParent, $addMemberRealName, $addMemberCountryId, $addMemberMobileNo;
    //修改
    public $modifyMid, $modifyUsername, $modifyRealName, $modifyCountryId, $modifyMobileNo, $modifyEmail, $modifyPassword, $modifyFundPassword, $modifyCryptoId = 0, $modifyCryptoAddress, $modifyBankId = 0, $modifyBankAccount, $modifyBankcard, $modifyMemo, $modifyGoogleAuthCode;

    //監聽註冊區間搜尋
    public function modifyRegisterRange($range)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateRange($range);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->registerRange = '';
        } else {
            $this->registerRange = $range;
        }
        //不更新畫面
        $this->skipRender();
    }

    public function mount()
    {
        $this->getDropdown();
        $searchParams = $this->getSearchParams();
        $this->getMemberData($searchParams);
    }

    public function render()
    {
        return view('livewire.member.member-list');
    }

    public function setTimeRange($option)
    {
        $common = new Common();
        $this->registerRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getMemberData($searchParams);
    }

    //分頁切換
    public function changePage($oldPage, $newPage)
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getMemberData($searchParams);
        }
    }

    //搜尋
    public function doSearch()
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getMemberData($searchParams);
    }

    //新增
    public function addMember()
    {
        //頁面傳參
        $addParams = [
            'type' => $this->addMemberType,
            'username' => $this->addMemberUsername,
            'password' => $this->addMemberPassword,
            'fund_password' => $this->addMemberFundPassword,
            'parent_username' => $this->addMemberParent,
            'real_name' => $this->addMemberRealName,
            'country_id' => $this->addMemberCountryId,
            'mobile' => $this->addMemberMobileNo,
        ];
        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $memberController = new MemberController();
        $addMember = $memberController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addMember->code) {
            redirect()->to('/memberList')->with('pagePopMessage', __('web_message.menu.member.addMember') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addMember->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addMember');
        }
    }

    //修改POP
    public function showModify($memberId)
    {
        if (!empty($memberId)) {
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['member_id' => $memberId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //用戶資訊
            $memberController = new MemberController();
            $memberData = $memberController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $memberData->code) {
                $this->modifyMid = $memberData->data->member->id;
                $this->modifyUsername = $memberData->data->member->username ?? '';
                $this->modifyRealName = $memberData->data->member->real_name ?? '';
                $this->modifyCountryId = $memberData->data->member->country_id ?? '';
                $this->modifyMobileNo = $memberData->data->member->mobile ?? '';
                $this->modifyEmail = $memberData->data->member->email ?? '';
                $this->modifyPassword = '';
                $this->modifyFundPassword = '';
                $this->modifyCryptoId = $memberData->data->crypto->bank_id ?? 0;
                $this->modifyCryptoAddress = $memberData->data->crypto->crypto_address ?? '';
                $this->modifyBankId = $memberData->data->bank->bank_id ?? 0;
                $this->modifyBankAccount = $memberData->data->bank->account_name ?? '';
                if(empty($this->modifyBankAccount) && !empty($this->modifyRealName)){
                    $this->modifyBankAccount = $this->modifyRealName;
                }
                $this->modifyBankcard = $memberData->data->bank->account_no ?? '';
                $this->modifyMemo = $memberData->data->member->memo ?? '';
                $this->modifyGoogleAuthCode = '';
                //展示POP
                session()->flash('showPopId', 'modifyMember');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (!empty($this->modifyMid)) {
            //抓取用戶資訊
            $common = new Common();
            $requests = $common->getRequest(['member_id' => $this->modifyMid]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $memberController = new MemberController();
            $memberData = $memberController->view($requests['request'])->getData();
            //串接修改request
            $modifyParams = [
                'member_id' => $this->modifyMid
            ];
            if (!empty($this->modifyRealName ?? '')) {
                if ($this->modifyRealName != ($memberData->data->member->real_name ?? '')) {
                    $modifyParams['real_name'] = $this->modifyRealName;
                }
            }
            if (!empty($this->modifyCountryId ?? '')) {
                if ($this->modifyCountryId != ($memberData->data->member->country_id ?? '')) {
                    $modifyParams['country_id'] = $this->modifyCountryId;
                }
            }
            if (!empty($this->modifyMobileNo ?? '')) {
                $modifyParams['country_id'] = $this->modifyCountryId;
                if ($this->modifyMobileNo != ($memberData->data->member->mobile ?? '')) {
                    $modifyParams['mobile'] = $this->modifyMobileNo;
                }
            }
            if (!empty($this->modifyEmail ?? '')) {
                if ($this->modifyEmail != ($memberData->data->member->email ?? '')) {
                    $modifyParams['email'] = $this->modifyEmail;
                }
            }
            if (!empty($this->modifyPassword ?? '')) {
                $modifyParams['password'] = $this->modifyPassword;
            }
            if (!empty($this->modifyFundPassword ?? '')) {
                $modifyParams['fund_password'] = $this->modifyFundPassword;
            }
            // 檢查任何一個加密錢包相關欄位是否被修改
            $cryptoInfoChanged = 
                ($this->modifyCryptoId != ($memberData->data->crypto->bank_id ?? 0) && !empty($this->modifyCryptoId ?? 0)) ||
                ($this->modifyCryptoAddress != ($memberData->data->crypto->crypto_address ?? '') && !empty($this->modifyCryptoAddress ?? ''));

            // 如果有任何一個加密錢包欄位被修改，則更新所有加密錢包資料
            if ($cryptoInfoChanged) {
                $modifyParams['crypto_id'] = $this->modifyCryptoId ?? ($memberData->data->crypto->bank_id ?? 0);
                $modifyParams['crypto_addr'] = $this->modifyCryptoAddress ?? ($memberData->data->crypto->crypto_address ?? '');
            }

            // 檢查任何一個銀行相關欄位是否被修改
            $bankInfoChanged = 
                ($this->modifyBankId != ($memberData->data->bank->bank_id ?? 0) && !empty($this->modifyBankId ?? 0)) ||
                ($this->modifyBankAccount != ($memberData->data->bank->account_name ?? '') && !empty($this->modifyBankAccount ?? '')) ||
                ($this->modifyBankcard != ($memberData->data->bank->account_no ?? '') && !empty($this->modifyBankcard ?? ''));

            // 如果有任何一個銀行欄位被修改，則更新所有銀行資料
            if ($bankInfoChanged) {
                $modifyParams['bank_id'] = $this->modifyBankId ?? ($memberData->data->bank->bank_id ?? 0);
                $modifyParams['bank_account'] = $this->modifyBankAccount ?? ($memberData->data->bank->account_name ?? '');
                $modifyParams['bank_no'] = $this->modifyBankcard ?? ($memberData->data->bank->account_no ?? '');
            }
            if ($this->modifyMemo != ($memberData->data->member->memo ?? '')) {
                $modifyParams['memo'] = $this->modifyMemo;
            }
            if (!empty($this->modifyGoogleAuthCode ?? '')) {
                $modifyParams['gauth_code'] = $this->modifyGoogleAuthCode;
            }
            $updateRequests = $common->getRequest($modifyParams);
            if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                $common->doLogout();
            }
            //修改接口
            $memberController = new MemberController();
            $updateMember = $memberController->update($updateRequests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $updateMember->code) {
                redirect()->to('/memberList')->with('pagePopMessage', __('web_message.menu.member.modifyMember') . ' ' . __('web_message.general.success'));
            } else {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', collect($updateMember->message ?? [])->implode('; '));
                //恢復原本POP
                session()->flash('popId', 'modifyMember');
            }
        }
    }

    //詳情
    public function doDetail($memberId)
    {
        if (!empty($memberId)) {
            session()->put('memberId', $memberId);
            redirect()->to('/memberDetail');
        }
    }

    public function doExport()
    {
        $searchParams = $this->getSearchParams();
        $searchParams['page'] = PublicEnum::DefaultExportPage;
        $searchParams['per_page'] = PublicEnum::DefaultExportPageSize;
        return Excel::download(new GenericExport('memberList', $searchParams), 'memberList' . time() . '.xlsx');
    }

    private function getDropdown(): void
    {
        //接口
        $common = new Common();
        $dropDownRequests = $common->getRequest([]);
        if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
            $common->doLogout();
        }
        //銀行及加密錢包id
        $bankController = new BankController();
        $dropDownResult = $bankController->getDropdown($dropDownRequests['request'])->getData();
        //手機國碼
        $mainController = new MainController();
        $countryCodeResult = $mainController->getDropdown($dropDownRequests['request'])->getData();
        //塞進session
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['bankIdList'] = $dropDownResult->data->bank ?? [];
        $pageCacheData['cryptoIdList'] = $dropDownResult->data->crypto ?? [];
        $pageCacheData['countryCodeList'] = $countryCodeResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->username ?? '')) {
            $searchParams['username'] = $this->username;
        }
        if ($this->fuzzy) {
            $searchParams['fuzzy_query'] = 1;
        }
        if (!empty($this->uid ?? 0)) {
            $searchParams['uid'] = $this->uid;
        }
        if (!empty($this->agentName ?? '')) {
            $searchParams['parent_username'] = $this->agentName;
        }
        if (!empty($this->realName ?? '')) {
            $searchParams['real_name'] = $this->realName;
        }
        if (!empty($this->countryId ?? 0)) {
            $searchParams['country_id'] = $this->countryId;
        }
        if (!empty($this->mobileNo ?? 0)) {
            $searchParams['mobile'] = $this->mobileNo;
        }
        if (!empty($this->referralCode ?? 0)) {
            $searchParams['invite_code'] = $this->referralCode;
        }
        if (!empty($this->registerIp ?? '')) {
            $searchParams['register_ip'] = $this->registerIp;
        }
        if (!empty($this->lastLoginIp ?? '')) {
            $searchParams['login_ip'] = $this->lastLoginIp;
        }
        if (!empty($this->memberType ?? 0)) {
            $searchParams['type'] = $this->memberType;
        }
        if (!empty($this->registerRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->registerRange);
            if (PublicEnum::ResponseCode['success'] == $dateCheck['code']) {
                $searchParams['start_time'] = $dateCheck['range']['start'];
                $searchParams['end_time'] = $dateCheck['range']['end'];
            }
        }
        if (!empty($this->accountNo ?? '')) {
            $searchParams['account_no'] = $this->accountNo;
        }
        if (!empty($this->cryptoAddr ?? '')) {
            $searchParams['crypto_addr'] = $this->cryptoAddr;
        }
        if (-1 != ($this->memberStatus ?? -1)) {
            $searchParams['status'] = $this->memberStatus;
        }
        $searchParams['page'] = PublicEnum::DefaultPage;
        if (!empty(session('nowPage') ?? 0)) {
            $searchParams['page'] = session('nowPage');
        }
        $searchParams['per_page'] = PublicEnum::DefaultPageSize;
        return $searchParams;
    }

    //請求資料
    private function getMemberData($params)
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $memberController = new MemberController();
        $memberList = $memberController->getList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $memberList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $memberList->data->list ?? [];
            $pageCacheData['totalPage'] = $memberList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $memberList->data->pagination->total ?? 0;
            session()->put('nowPage', $memberList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($memberList->message ?? [])->implode('; '));
        }
    }
}
