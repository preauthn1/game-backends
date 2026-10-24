<?php

namespace App\Livewire;

use Illuminate\Support\Facades\App;
use Livewire\Component;
use App\Enums\PublicEnum;
use Illuminate\Http\Request;
use App\Http\Controllers\Auth\LoginController;
use Exception;

class Login extends Component
{
    public $username;
    public $password;

    public function render()
    {
        //語言
        $language = session()->get('language') ?? config('app.locale');
        App::setLocale($language);
        return view('livewire.login');
    }

    //修改語言
    public function changeLanguage($languageCode)
    {
        $common = new Common();
        $common->changeLanguage($languageCode);
    }

    //登入
    public function doLogin()
    {
        try {
            //組合傳參
            $request = new Request();
            $request->replace(['username' => $this->username, 'password' => $this->password]);
            //調用接口
            $loginController = new LoginController();
            $result = $loginController->login($request)->getData();
            if (PublicEnum::ResponseCode['success'] == $result->code) {
                session()->put('userData', $result->data);
                redirect()->to('/dashboard');
            } else {
                session()->flash('loginErrorMessage', collect($result->message)->implode('; '));
            }
        } catch (Exception $exception) {
            session()->flash('loginErrorMessage', '登入錯誤');
        }
    }
}
