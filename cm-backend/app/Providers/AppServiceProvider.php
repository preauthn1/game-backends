<?php

namespace App\Providers;

use Illuminate\Support\ServiceProvider;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\Validator;
use Illuminate\Database\Eloquent\Builder;

class AppServiceProvider extends ServiceProvider
{
    /**
     * Register any application services.
     */
    public function register(): void
    {
        //
    }

    /**
     * Bootstrap any application services.
     */
    public function boot(): void
    {
        Schema::defaultStringLength(191);
        // 添加自定義驗證規則,帳號只允许字母、數字, 帳號中間必需要有一個字母
        Validator::extend('r_username', function ($attribute, $value, $parameters, $validator) {
            return is_string($value) && preg_match('/^[A-Za-z0-9]*[A-Za-z]+[A-Za-z0-9]*$/u', $value);
        });
        // 添加自定義驗證規則,允许字母、數字和-
        Validator::extend('r_alpha_num', function ($attribute, $value, $parameters, $validator) {
            return is_string($value) && preg_match('/^[A-Za-z0-9-]+$/u', $value);
        });
        // 添加自定義驗證規則,允许字母、數字和_-空白
        Validator::extend('r_alpha_dash', function ($attribute, $value, $parameters, $validator) {
            return is_string($value) && preg_match('/^[A-Za-z0-9-_\s]+$/u', $value);
        });
        Validator::extend('domain', function ($attribute, $value, $parameters, $validator) {
            return is_string($value) && preg_match('/^[A-Za-z0-9.]+$/u', $value);
        });
        Validator::extend('float', function ($attribute, $value, $parameters, $validator) {
            return preg_match('/^\d{0,6}.?\d{0,5}$/u', $value);
        });
        Validator::extend('fuzzy_ip', function ($attribute, $value, $parameters, $validator) {
            return preg_match('/^(((?:((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|\*)\.)){3}((?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)|\*))*$/'
                , $value);
        });
        Validator::extend('base64image', function ($attribute, $value, $parameters, $validator) {
            $value = str_replace(' ', '+', $value);
            $explode = explode(',', $value);
            $pos = strpos($value, ';');
            $type = explode(':', substr($value, 0, $pos))[1];
            $mime = explode('/', $type);
            $allow = [
                'png',
                'jpg',
                'jpeg',
                'gif',
                'svg'
            ];
            // check file format
            if (! in_array(strtolower($mime[1]), $allow)) {
                return false;
            }
            // check base64 format
            if (! preg_match('%^[a-zA-Z0-9/+]*={0,2}$%', $explode[1])) {
                return false;
            }
            return true;
        });

        Builder::macro('toRawSql', function(){
            return array_reduce($this->getBindings(), function($sql, $binding){
                return preg_replace('/\?/', is_numeric($binding) ? $binding : "'".$binding."'" , $sql, 1);
            }, $this->toSql());
        });

        Builder::macro('toRawSql', function(){
            return ($this->getQuery()->toRawSql());
        });

        Builder::macro('toPaginate', function($pageSize) {
            $paginate = $this->paginate($pageSize)->toArray();
            return [
                $paginate['data'],
                [
                    'current_page' => $paginate['current_page'],
                    'per_page' => $paginate['per_page'],
                    'last_page' => $paginate['last_page'],
                    'total' => $paginate['total'],
                    'from' => $paginate['from'],
                    'to' => $paginate['to']
                ]
            ];
        });

        Builder::macro('toPaginate', function($pageSize){
            return ($this->getQuery()->toPaginate($pageSize));
        });
    }
}
