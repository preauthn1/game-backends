<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'agents';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->integer('host_id')->default(0)->comment('主帳號的系統編號(不等於0就是子帳號)');
                $table->integer('parent')->comment('上層代理系統編號(Agent)');
                $table->tinyInteger('role_id')->default(0)->comment('roles.id');
                $table->string('hierarchy_path', 500)->collation('utf8mb4_unicode_ci')->nullable()->comment('新版hierarchy結構，存成路徑式的數值');
                $table->string('code', 10)->collation('utf8mb4_unicode_ci')->comment('邀請碼');
                $table->string('username', 32)->collation('utf8mb4_unicode_ci')->comment('姓名');
                $table->string('nickname', 191)->collation('utf8mb4_unicode_ci')->nullable()->comment('暱稱');
                $table->string('email', 50)->collation('utf8mb4_unicode_ci')->comment('電子信箱');
                $table->string('password', 128)->collation('utf8mb4_unicode_ci')->comment('密碼');
                $table->string('payment_password', 128)->collation('utf8mb4_unicode_ci')->nullable()->comment('支付密碼');
                $table->string('im_certify', 191)->collation('utf8mb4_unicode_ci')->nullable()->comment('vworld影子帳號密碼');
                $table->string('mobile', 30)->collation('utf8mb4_unicode_ci')->nullable()->comment('聯繫號碼');
                $table->tinyInteger('frontend_register')->default(0)->comment('前端是否可以註冊(0:不可註冊,1:可註冊)');
                $table->tinyInteger('has_2fa')->default(0);
                $table->tinyInteger('email_validate')->default(0)->comment('電子郵件是否需要認證0:不需要,1:需要');
                $table->tinyInteger('mobile_validate')->default(0)->comment('電話號碼是否需要認證0:不需要,1:需要');
                $table->tinyInteger('mobile_bind')->default(0)->comment('電話號碼是否綁定');
                $table->tinyInteger('status')->default(1)->comment('帳號狀態 0:鎖定,1:正常');
                $table->timestamp('last_login_time')->nullable()->comment('最後登入時間');
                $table->bigInteger('last_login_ip')->default(0)->comment('最後登入IP');
                $table->string('memo', 191)->collation('utf8mb4_unicode_ci')->nullable()->comment('方便技術記錄用');
                $table->integer('organization_id')->default(1)->comment('所屬組織');
                $table->integer('customized_setting_group_id')->default(1)->comment('個性化設置_組別ID (agent_groups 資料表)');
                $table->integer('cash_flow_group_id')->default(2)->comment('群組編號');
                $table->integer('sms_gateway_group_id')->default(3)->comment('簡訊群組ID');
                $table->integer('miner_group_id')->default(4)->comment('礦機群組編號');
                $table->timestamp('last_reset_password')->nullable()->comment('最後更新密碼時間');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->integer('created_by')->default(0)->comment('創建代理的操作人');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');
                $table->integer('deleted_at')->default(0);

                //unique
                $table->unique('username', 'uni_agents_username');
                //index
                $table->index('parent', 'idx_' . $this->table_name . '_' . 'parent');
                $table->index('hierarchy_path', 'idx_' . $this->table_name . '_' . 'hierarchy_path');
                $table->index('customized_setting_group_id', 'idx_' . $this->table_name . '_' . 'customized_setting_group_id');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'host_id' => 0, 'parent' => 0, 'role_id' => 1, 'hierarchy_path' => '/1', 'code' => 'TMZ8779',
                    'username' => 'Admin', 'password' => '$2y$10$QlelXrbe3MfMmPMTzTJX3OliBOOKyyoAnwMjKytTrtBDXweHiae1C',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 51, 'host_id' => 1, 'parent' => 1, 'role_id' => 1, 'hierarchy_path' => '/1/51', 'code' => '-',
                    'username' => 'System', 'password' => '$2y$10$8z2ZTZB.xgRBcT5JqBzpkOMP2Sx412EliDMYI59RqfvzI/bxWQcMG',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 52, 'host_id' => 1, 'parent' => 1, 'role_id' => 1, 'hierarchy_path' => '/1/52', 'code' => '-',
                    'username' => 'Auto', 'password' => '$2y$10$8z2ZTZB.xgRBcT5JqBzpkOMP2Sx412EliDMYI59RqfvzI/bxWQcMG',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100001, 'host_id' => 0, 'parent' => 1, 'role_id' => 11, 'hierarchy_path' => '/1/100001', 'code' => '11111',
                    'username' => 'mybit', 'password' => '$2y$10$Unqq0p8bDdcTOvph5qCzyuH3JHBQxbLt5yjY7bFncbfOT861BenHK',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100003, 'host_id' => 100001, 'parent' => 100001, 'role_id' => 11, 'hierarchy_path' => '/1/100001/100003', 'code' => '-',
                    'username' => 'test01', 'password' => '$2y$12$.iKEspWEPI93u05ZNAD4SOYvs3wP036ziqaaNGuX1bPLNajXMtbe.',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100004, 'host_id' => 0, 'parent' => 1, 'role_id' => 11, 'hierarchy_path' => '/1/100004', 'code' => '927SD5',
                    'username' => 'test02', 'password' => '$2y$12$deZNwobHb/TIjXWyJr/JNuKJAU2Sb0QgSTqcDvePJM3PXpvT4iPji',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100005, 'host_id' => 100003, 'parent' => 100003, 'role_id' => 11, 'hierarchy_path' => '/1/100001/100003/100005', 'code' => '-',
                    'username' => 'test01sub', 'password' => '$2y$12$7X8OOHPPyA0yrcXYcKMvje3uZcwY.c5.Qx9897Z.E0Q3UqvQ3jNx2',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100006, 'host_id' => 100003, 'parent' => 100003, 'role_id' => 11, 'hierarchy_path' => '/1/100001/100003/100006', 'code' => '-',
                    'username' => 'test01sub1', 'password' => '$2y$12$RUihYb9oUidsCLjB3AzVf.egTbg2hkZZwEvsaIUPGRP5MpWhQdwVi',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100007, 'host_id' => 100001, 'parent' => 100001, 'role_id' => 11, 'hierarchy_path' => '/1/100001/100007', 'code' => '-',
                    'username' => 'leah', 'password' => '$2y$12$8Sc./S/QPUbEd69iNBD88.ToiRRBObmgsXFJ7ALQKGy6oo9JqihRi',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100008, 'host_id' => 0, 'parent' => 1, 'role_id' => 11, 'hierarchy_path' => '/1/100008', 'code' => '973HN4',
                    'username' => 'cm01', 'password' => '$2y$12$tRwJUhlre1wke.5QVdqU4eE8B2fmXLlQzgLCUnW5JxJIUhW5zh2zW',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100009, 'host_id' => 100008, 'parent' => 100008, 'role_id' => 11, 'hierarchy_path' => '/1/100008/100009', 'code' => '-',
                    'username' => 'cm01sub01', 'password' => '$2y$12$r0uw5SI1UngHgCFO3qlo7OP1beUUGG9K/MnvayA4xp.BNyfR8Wywy',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100010, 'host_id' => 100008, 'parent' => 100008, 'role_id' => 11, 'hierarchy_path' => '/1/100008/100010', 'code' => '-',
                    'username' => 'cm01sub02', 'password' => '$2y$12$q1pTwhp8CA4ues51IaFPzeSloJkal7nnjy/VDp6x.CAl5UHlCYgW2',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100011, 'host_id' => 100008, 'parent' => 100008, 'role_id' => 11, 'hierarchy_path' => '/1/100008/100011', 'code' => '-',
                    'username' => 'cm01sub03', 'password' => '$2y$12$JgeiXV2uEjurbcQ9WX4SQOarEk3krX7JeoFmWzF3Og82qG.1Nx7.q',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100012, 'host_id' => 100001, 'parent' => 100001, 'role_id' => 11, 'hierarchy_path' => '/1/100001/100012', 'code' => '-',
                    'username' => 'mybitsub00', 'password' => '$2y$12$EgH5G0puIeXmmV2PP8UORuxeKY4uPKArfMOz4TERiXo6hrPh2mptS',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100013, 'host_id' => 100001, 'parent' => 100001, 'role_id' => 11, 'hierarchy_path' => '/1/100001/100013', 'code' => '-',
                    'username' => 'mybitsub01', 'password' => '$2y$12$66Y/yOw6p2X/AM/sHVem6OCxx061JZByO19YJh3Ke6kf/Eqg48lyK',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100014, 'host_id' => 100001, 'parent' => 100001, 'role_id' => 11, 'hierarchy_path' => '/1/100001/100014', 'code' => '-',
                    'username' => 'mybitsub02', 'password' => '$2y$12$wbrZT9mngUiq2GUUkIO2WeSaatjqZ/OEUmwUzRLPS6mGy.67cI1IO',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100015, 'host_id' => 100001, 'parent' => 100001, 'role_id' => 11, 'hierarchy_path' => '/1/100001/100015', 'code' => '-',
                    'username' => 'mybitsub03', 'password' => '$2y$12$TqO9OTA0aZqKhXL4SeN77uHXXDyEbxLJ9GYnVlcAmb0QGN/L3k.9O',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100016, 'host_id' => 100001, 'parent' => 100001, 'role_id' => 11, 'hierarchy_path' => '/1/100001/100016', 'code' => '-',
                    'username' => 'rdsub05', 'password' => '$2y$12$4nUkTyTIeYpIalQIqL3n/./FnUZAzpKQZl9Y8t4cnjrE1Qu1XQU2u',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100017, 'host_id' => 100001, 'parent' => 100001, 'role_id' => 11, 'hierarchy_path' => '/1/100001/100017', 'code' => '-',
                    'username' => 'leahpay', 'password' => '$2y$12$5DnQ2w1rRJw9Qn.Woj0qgu5i7u5mcOwI6qPfcncGLZ3kXnjsjuqfG',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
                ['id' => 100018, 'host_id' => 100001, 'parent' => 100001, 'role_id' => 11, 'hierarchy_path' => '/1/100001/100018', 'code' => '-',
                    'username' => 'leahcs', 'password' => '$2y$12$r4oUAgS2lgfOVGshujaUqObiJ./LlAlBFKuYO1X4BkmQZLqbOIzE6',
                    'customized_setting_group_id' => 1, 'cash_flow_group_id' => 2, 'sms_gateway_group_id' => 3, 'miner_group_id' => 4],
            ]);
        }
    }

    /**
     * Reverse the migrations.
     */
    public function down(): void
    {
        Schema::dropIfExists($this->table_name);
    }
};
