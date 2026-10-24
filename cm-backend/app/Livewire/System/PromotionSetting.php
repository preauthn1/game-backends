<?php

namespace App\Livewire\System;

use App\Enums\PublicEnum;
use App\Http\Controllers\Promotion\MainController;
use App\Livewire\Common;
use Livewire\Component;
use App\Helpers\Common as Helper;

class PromotionSetting extends Component
{
    //搜尋
    public $promotionType, $promotionRange, $promotionStatus;
    //監聽時間相關
    protected $listeners = [
        'promotionRange' => 'modifyPromotionRange',
        'promotionStartTime' => 'promotionStartTime',
        'promotionEndTime' => 'promotionEndTime',
        'addImage' => 'handleAddImage',
        'addImageContent' => 'handleAddImageContent',
        'modifyImage' => 'handleModifyImage',
        'modifyImageContent' => 'handleModifyImageContent',
    ];
    //新增
    public $addPromotionType=0, $addPromotionTitle, $addPromotionContent, $addPromotionUrl, $addImage, $addImageContent, $addPromotionStatus = true, $promotionStartTime = '', $promotionEndTime = '';
    //修改
    public $modifyPromotionId, $modifyPromotionType, $modifyPromotionTitle, $modifyPromotionContent, $modifyPromotionUrl, $modifyImage, $modifyImageContent, $modifyPromotionStatus, $modifyPromotionSort;

    //監聽新增活動圖片
    public function handleAddImage($data)
    {
        $this->addImage = $data;
        //不更新畫面
        $this->skipRender();
    }

    //監聽新增活動圖片內容
    public function handleAddImageContent($data)
    {
        $this->addImageContent = $data;
        //不更新畫面
        $this->skipRender();
    }

    //監聽更新活動圖片
    public function handleModifyImage($data)
    {
        $this->modifyImage = $data;
        //不更新畫面
        $this->skipRender();
    }

    //監聽更新活動圖片內容
    public function handleModifyImageContent($data)
    {
        $this->modifyImageContent = $data;
        //不更新畫面
        $this->skipRender();
    }

    //監聽時間區間搜尋
    public function modifyPromotionRange($range)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateRange($range);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->promotionRange = '';
        } else {
            $this->promotionRange = $range;
        }
        //不更新畫面
        $this->skipRender();
    }

    //監聽開始時間
    public function promotionStartTime($date)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateTime($date);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->promotionStartTime = '';
        } else {
            $this->promotionStartTime = $date;
        }
        //不更新畫面
        $this->skipRender();
    }

    //監聽結束時間
    public function promotionEndTime($date)
    {
        //驗證日期
        $common = new Common();
        $dateCheck = $common->checkDateTime($date);
        if (PublicEnum::ResponseCode['success'] != $dateCheck['code']) {
            session()->flash('pagePopMessage', $dateCheck['message']);
            $this->promotionEndTime = '';
        } else {
            $this->promotionEndTime = $date;
        }
        //不更新畫面
        $this->skipRender();
    }

    public function mount()
    {
        $this->typeDropdown();
        $searchParams = $this->getSearchParams();
        $this->getPromotionData($searchParams);
    }

    public function render()
    {
        return view('livewire.system.promotion-setting');
    }

    public function setTimeRange($option)
    {
        $common = new Common();
        $this->promotionRange = $common->fastTimeRange($option);
        $searchParams = $this->getSearchParams();
        $this->getPromotionData($searchParams);
    }

    //分頁切換
    public function changePage($oldPage, $newPage)
    {
        if ($oldPage != $newPage) {
            if (1 > $newPage) $newPage = 1;
            if ($newPage > session()->get('pageCacheData')['totalPage']) $newPage = session()->get('pageCacheData')['totalPage'];
            session()->put('nowPage', ($newPage ?? PublicEnum::DefaultPage));
            $searchParams = $this->getSearchParams();
            $this->getPromotionData($searchParams);
        }
    }

    //搜尋
    public function doSearch()
    {
        session()->put('nowPage', PublicEnum::DefaultPage);
        $searchParams = $this->getSearchParams();
        $this->getPromotionData($searchParams);
    }

    //新增
    public function addPromotion()
    {
        //頁面傳參
        $addParams = [
            'type' => $this->addPromotionType,
            'title' => $this->addPromotionTitle,
            'image' => $this->addImage,
            'start_time' => $this->promotionStartTime,
            'end_time' => $this->promotionEndTime,
            'status' => $this->addPromotionStatus ? 1 : 0,
        ];

        if (Helper::IsNotNullOrEmptyString($this->addPromotionContent)) {
            $addParams['content'] = $this->addPromotionContent;
        }

        if (Helper::IsNotNullOrEmptyString($this->addPromotionUrl)) {
            $addParams['url'] = $this->addPromotionUrl;
        }

        if (Helper::IsNotNullOrEmptyString($this->addImageContent)) {
            $addParams['image_content'] = $this->addImageContent;
        }

        //串接request
        $common = new Common();
        $requests = $common->getRequest($addParams);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        //新增接口
        $promotionController = new MainController();
        $addPromotion = $promotionController->create($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $addPromotion->code) {
            redirect()->to('/promotionSetting')->with('pagePopMessage', __('web_message.menu.system.addPromotion') . ' ' . __('web_message.general.success'));
        } else {
            //顯示錯誤訊息POP
            session()->flash('pagePopMessage', collect($addPromotion->message ?? [])->implode('; '));
            //恢復原本POP
            session()->flash('popId', 'addPromotion');
        }
    }

    //修改POP
    public function showDataPop($promotionId, $action = '')
    {
        if (!empty($promotionId) &&!empty($action)) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['popAction'] = $action;
            session()->put('pageCacheData', $pageCacheData);
            //串接request
            $common = new Common();
            $requests = $common->getRequest(['id' => $promotionId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            //資訊
            $promotionController = new MainController();
            $promotionData = $promotionController->view($requests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $promotionData->code) {
                $this->modifyPromotionId = $promotionData->data->data->id;
                $this->modifyPromotionType = $promotionData->data->data->type ?? 0;
                $this->modifyPromotionTitle = $promotionData->data->data->title ?? '';
                $this->modifyPromotionContent = $promotionData->data->data->content ?? '';
                $this->modifyPromotionUrl = $promotionData->data->data->url ?? '';
                $this->modifyImage = $promotionData->data->data->image ?? '';
                $this->modifyImageContent = $promotionData->data->data->image_content ?? '';
                $this->promotionStartTime = $promotionData->data->data->start_time ?? '';
                $this->promotionEndTime = $promotionData->data->data->end_time ?? '';
                $this->modifyPromotionSort = $promotionData->data->data->sort ?? 0;
                $this->modifyPromotionStatus = (1 == ($promotionData->data->data->status ?? 1));
                //展示POP
                session()->flash('showPopId', 'modifyPromotion');
            }
        }
    }

    //修改
    public function doModify()
    {
        if (!empty($this->modifyPromotionId)) {
            //抓取資訊
            $common = new Common();
            $requests = $common->getRequest(['id' => $this->modifyPromotionId]);
            if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
                $common->doLogout();
            }
            $promotionController = new MainController();
            $promotionData = $promotionController->view($requests['request'])->getData();
            //串接修改request
            $modifyParams = [];
            if ($this->modifyPromotionType != ($promotionData->data->data->type ?? 0)) {
                $modifyParams['type'] = $this->modifyPromotionType;
            }
            if ($this->modifyPromotionTitle != ($promotionData->data->data->title ?? '')) {
                $modifyParams['title'] = $this->modifyPromotionTitle;
            }
            if ($this->modifyPromotionContent != ($promotionData->data->data->content ?? '')) {
                $modifyParams['content'] = $this->modifyPromotionContent;
            }
            if ($this->modifyPromotionUrl != ($promotionData->data->data->url ?? '')) {
                $modifyParams['url'] = $this->modifyPromotionUrl;
            }
            if ($this->modifyImage != ($promotionData->data->data->image ?? '')) {
                $modifyParams['image'] = $this->modifyImage;
            }
            if ($this->modifyImageContent != ($promotionData->data->data->image_content ?? '')) {
                $modifyParams['image_content'] = $this->modifyImageContent;
            }
            if ($this->promotionStartTime != ($promotionData->data->data->start_time ?? '')) {
                $modifyParams['start_time'] = $this->promotionStartTime;
            }
            if ($this->promotionEndTime != ($promotionData->data->data->end_time ?? '')) {
                $modifyParams['end_time'] = $this->promotionEndTime;
            }
            if ($this->modifyPromotionSort != ($promotionData->data->data->sort ?? 0)) {
                $modifyParams['sort'] = $this->modifyPromotionSort;
            }
            if ($this->modifyPromotionStatus != (1 == ($promotionData->data->data->status ?? 1))) {
                $modifyParams['status'] = $this->modifyPromotionStatus ? 1 : 0;
            }
            //無異動
            if (empty($modifyParams)) {
                //顯示錯誤訊息POP
                session()->flash('pagePopMessage', __('web_message.general.noChange'));
                //恢復原本POP
                session()->flash('popId', 'modifyPromotion');
            } else {
                $modifyParams = array_merge(['id' => $this->modifyPromotionId], $modifyParams);
                $modifyParams['original_url'] = $promotionData->data->data->url;
                $modifyParams['original_image_content'] = $promotionData->data->data->image_content;
                $updateRequests = $common->getRequest($modifyParams);
                if (PublicEnum::ResponseCode['needLogin'] == $updateRequests['code']) {
                    $common->doLogout();
                }
                //修改接口
                $updatePromotion = $promotionController->update($updateRequests['request'])->getData();
                if (PublicEnum::ResponseCode['success'] == $updatePromotion->code) {
                    $searchParams = $this->getSearchParams();
                    $this->getPromotionData($searchParams);
                    session()->flash('pagePopMessage', __('web_message.menu.system.modifyPromotion') . ' ' . __('web_message.general.success'));
                } else {
                    //顯示錯誤訊息POP
                    session()->flash('pagePopMessage', collect($updatePromotion->message ?? [])->implode('; '));
                    //恢復原本POP
                    session()->flash('popId', 'modifyPromotion');
                }
            }
        }
    }

    //搜尋參數串接
    private function getSearchParams(): array
    {
        $searchParams = [];
        if (!empty($this->promotionRange ?? '')) {
            $common = new Common();
            $dateCheck = $common->checkDateRange($this->promotionRange);
            if (PublicEnum::ResponseCode['success'] == $dateCheck['code']) {
                $searchParams['start_time'] = $dateCheck['range']['start'];
                $searchParams['end_time'] = $dateCheck['range']['end'];
            }
        }
        if (-1 != ($this->promotionStatus ?? -1)) {
            $searchParams['status'] = $this->promotionStatus;
        }

        if (-1 != ($this->promotionType ?? -1)) {
            $searchParams['type'] = $this->promotionType;
        }

        $searchParams['page'] = PublicEnum::DefaultPage;
        if (!empty(session('nowPage') ?? 0)) {
            $searchParams['page'] = session('nowPage');
        }
        $searchParams['per_page'] = PublicEnum::DefaultPageSize;
        return $searchParams;
    }

    private function typeDropdown(): void
    {
        //接口
        $common = new Common();
        $dropDownRequests = $common->getRequest([]);
        if (PublicEnum::ResponseCode['needLogin'] == $dropDownRequests['code']) {
            $common->doLogout();
        }
        $promotionController = new MainController();
        $dropDownResult = $promotionController->getDropdown($dropDownRequests['request'])->getData();
        $pageCacheData = session()->get('pageCacheData') ?? [];
        $pageCacheData['typeList'] = $dropDownResult->data->list ?? [];
        session()->put('pageCacheData', $pageCacheData);
    }

    private function getPromotionData($params)
    {
        $common = new Common();
        $requests = $common->getRequest($params);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $promotionController = new MainController();
        $promotionList = $promotionController->getList($requests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $promotionList->code) {
            $pageCacheData = session()->get('pageCacheData') ?? [];
            $pageCacheData['pageData'] = $promotionList->data->list ?? [];
            $pageCacheData['totalPage'] = $promotionList->data->pagination->pages ?? PublicEnum::DefaultPage;
            $pageCacheData['totalRow'] = $promotionList->data->pagination->total ?? 0;
            session()->put('nowPage', $promotionList->data->pagination->current ?? PublicEnum::DefaultPage);
            session()->put('pageCacheData', $pageCacheData);
        } else {
            session()->flash('pagePopMessage', collect($promotionList->message ?? [])->implode('; '));
        }
    }
}
