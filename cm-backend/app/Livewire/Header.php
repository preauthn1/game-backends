<?php

namespace App\Livewire;

use Livewire\Component;
use App\Livewire\Common;

class Header extends Component
{
    public function render()
    {
        return view('livewire.header');
    }

    //修改語言
    public function changeLanguage($languageCode)
    {
        $common = new Common();
        $common->changeLanguage($languageCode);
        $this->js('window.location.reload()');
    }

    //登出
    public function doLogout()
    {
        $common = new Common();
        $common->doLogout();
    }
}
