<?php

namespace App\Livewire;

use Livewire\Component;
use App\Livewire\Common;
use App\Http\Controllers\Auth\MenuController;
use App\Enums\PublicEnum;

class Menu extends Component
{
    public function mount()
    {
        $userData = session()->get('userData') ?? [];
        if (empty($userData->menu ?? []) || empty($userData->totalUri ?? [])) {
            //接口
            $common = new Common();
            $infoRequests = $common->getRequest([]);
            if (PublicEnum::ResponseCode['needLogin'] == $infoRequests['code']) {
                $common->doLogout();
            }
            $menuController = new MenuController();
            $menuResult = $menuController->getList($infoRequests['request'])->getData();
            if (PublicEnum::ResponseCode['success'] == $menuResult->code) {
                $userData = session()->get('userData') ?? [];
                $userData->menu = $menuResult->data->list ?? [];
                $userData->totalUri = $menuResult->data->totalUri ?? [];
                session()->put('userData', $userData);
            } else {
                session()->flash('pagePopMessage', collect($dataList->message ?? [])->implode('; '));
            }
        }
    }

    public function render()
    {
        return view('livewire.menu');
    }
}
