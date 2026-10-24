<?php

namespace App\Services;

use App\Helpers\Common;
use App\Repositories\GroupDefineRepository;
use App\Repositories\AgentSettingRepository;
use Exception;
use App\Enums\PublicEnum;
use Illuminate\Support\Facades\DB;

class GroupDefineService extends BaseService
{
    public function __construct()
    {
        $this->repository = new GroupDefineRepository();
    }

    public function getList($params)
    {
        $mainParams = $params;
        $mainParams->parent_id = 0;
        $mainResult = $this->repository->getList($mainParams);
        $nowLanguage = session()->get('language') ?? config('app.locale');
        $languageId = PublicEnum::Language[$nowLanguage]['id'];
        return $mainResult->map(function ($item) use ($params, $languageId) {
            $itemLanguageList = json_decode($item->language_text, true);
            foreach ($itemLanguageList ?? [] as $language) {
                if ($language['id'] == $languageId) {
                    $item->name = $language['txt'];
                }
            }
            $params->parent_id = $item->define_id;
            $subResult = $this->repository->getList($params);
            $item->sub = $subResult->map(function ($subItem) use ($languageId) {
                $subItemLanguageList = json_decode($subItem->language_text, true);
                foreach ($subItemLanguageList ?? [] as $language) {
                    if ($language['id'] == $languageId) {
                        $subItem->name = $language['txt'];
                    }
                }
                $subItem->dataTypeTxt = __('param.data_type.' . $subItem->data_type);
                return $subItem;
            });
            return $item;
        });
    }

    public function view($params)
    {
        $result = $this->repository->view($params->define_id);
        if (!is_object($result)) {
            throw new Exception(__('error_message.9001'), 9001);
        }
        return $result;
    }

    public function create($params)
    {
        $data = array();
        $defineData = $this->repository->findByDefineId($params->define_id);
        if (!$defineData) {
            throw new Exception(__('error_message.5049'), 5049);
        } else {
            $data['define_id'] = $params->define_id;
        }
        if (Common::IsNotNullOrEmptyString($params->parent_define_id)) {
            $parentDefineData = $this->repository->findByDefineId($params->parent_define_id);
            if (!$parentDefineData) {
                throw new Exception(__('error_message.5048'), 5048);
            } else {
                $data['parent_id'] = $params->parent_define_id;
            }
        }
        $data['group_type'] = $params->group_type;
        $data['organization_id'] = empty($params->ORGANIZATION_ID ?? '') ? 1 : $params->ORGANIZATION_ID;
        if (Common::IsNotNullOrEmptyString($params->data_type)) {
            $data['data_type'] = $params->data_type;
        }
        if (Common::IsNotNullOrEmptyString($params->default_value)) {
            $data['default_value'] = $params->default_value;
        }
        if (Common::IsNotNullOrEmptyString($params->language_text)) {
            $data['language_text'] = $params->language_text;
        }
        if (Common::IsNotNullOrEmptyString($params->memo)) {
            $data['memo'] = $params->memo;
        }
        $result = $this->repository->create($data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        //檢查對應群組是否都有此子定義 沒有的需要新增setting紀錄
        if (0 < $params->parent_define_id) {
            $this->checkGroupSettingEmptyCreate($params);
        }
    }

    public function update($params)
    {
        $define = $this->repository->findByDefineId($params->define_id);
        if (!is_object($define)) {
            throw new Exception(__('error_message.5049'), 5049);
        }
        $data = array();
        if (isset($params->default_value)) {
            $data['default_value'] = $params->default_value;
        }
        if (Common::IsNotNullOrEmptyString($params->language_text)) {
            $data['language_text'] = $params->language_text;
        }
        if (isset($params->memo)) {
            $data['memo'] = $params->memo;
        }
        if (count($data) > 0) {
            $result = $this->repository->updateByDefineId($params->define_id, $data);
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
            //檢查對應群組是否都有此定義 沒有的需要新增setting紀錄
            if (0 < $define->get()[0]->parent_id) {
                $params->group_type = $define->get()[0]->group_type;
                $this->checkGroupSettingEmptyCreate($params);
            }
        }
    }

    //對應群組是否都有此定義 沒有的需要新增setting紀錄
    private function checkGroupSettingEmptyCreate($params)
    {
        if (!empty($params->group_type ?? '') && !empty($params->define_id ?? '')) {
            $addSetting = [];
            $defineData = $this->repository->view($params->define_id);
            $defaultValue = $defineData->default_value ?? '';
            $agentSettingRepository = new AgentSettingRepository();
            $settingList = $agentSettingRepository->getGroupSettingIdList($params)->get();
            $settingList->map(function ($item) use ($params, &$defaultValue, &$addSetting) {
                if (empty($item->setting_id ?? '')) {
                    $addSetting[] = [
                        'setting_key' => $params->define_id,
                        'agent_group_id' => $item->group_id,
                        'value' => $defaultValue,
                    ];
                }
            });
            DB::table('agent_customized_settings')->insert($addSetting);
        }
    }
}
