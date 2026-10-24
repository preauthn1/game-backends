<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'members';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號(UID)');
                $table->tinyInteger('type')->default(1)->comment('玩家類型; 1:正常玩家, 2:內部測試會員, 3:外部測試會員, 9:特殊帳號(註冊用)');
                $table->integer('uid')->default(0);
                $table->integer('agent_id')->comment('代理系統編號(agents.id)');
                $table->string('username', 32)->collation('utf8mb4_unicode_ci')->comment('姓名');
                $table->string('nickname', 191)->collation('utf8mb4_unicode_ci')->nullable()->comment('暱稱');
                $table->integer('avatar_id')->default(1)->comment('頭像系統編號(avatar.id)');
                $table->integer('level')->default(0)->comment('層級');
                $table->string('password', 128)->collation('utf8mb4_unicode_ci')->comment('密碼');
                $table->string('fund_password', 128)->collation('utf8mb4_unicode_ci')->nullable()->comment('支付密碼');
                $table->string('referral_code', 20)->collation('utf8mb4_unicode_ci')->nullable()->comment('邀請碼');
                $table->string('invitation_code', 10)->collation('utf8mb4_unicode_ci')->nullable();
                $table->integer('is_validate')->default(0)->comment('是否通過身份驗證');
                $table->tinyInteger('email_validate')->default(0)->comment('狀態 0:未認證,1:已認證');
                $table->string('email', 60)->collation('utf8mb4_unicode_ci')->nullable()->comment('電子信箱');
                $table->tinyInteger('mobile_validate')->default(0)->comment('是否通過手機驗證');
                $table->string('mobile_country_code', 4)->collation('utf8mb4_unicode_ci')->nullable()->comment('電話號碼國家代碼');
                $table->string('mobile', 32)->collation('utf8mb4_unicode_ci')->nullable()->comment('電話號碼');
                $table->integer('country_id')->nullable()->comment('電話國碼(Country.id)');
                $table->text('device_id')->collation('utf8mb4_unicode_ci')->comment('推播設備號');
                $table->string('device_type', 20)->collation('utf8mb4_unicode_ci')->nullable()->comment('設備種類');
                $table->string('real_name', 50)->collation('utf8mb4_unicode_ci')->nullable()->comment('first name');
                $table->integer('member_id_card_id')->default(0)->comment('會員身份證系統編號(member_id_cards.id)');
                $table->text('id_card_front')->collation('utf8mb4_unicode_ci')->comment('身份證正面');
                $table->text('id_card_back')->collation('utf8mb4_unicode_ci')->comment('身份證反面');
                $table->tinyInteger('is_2fa_enable')->default(0)->comment('Google驗證器開關, 0:關閉, 1:開啟');
                $table->string('google2fa_secret', 100)->collation('utf8mb4_unicode_ci')->nullable()->comment('Google驗證器開關代碼');
                $table->integer('register_country_id')->nullable()->comment('註冊國家');
                $table->bigInteger('register_ip')->nullable()->comment('註冊IP');
                $table->string('register_domain', 100)->collation('utf8mb4_unicode_ci')->nullable()->comment('註冊網域');
                $table->tinyInteger('status')->default(0)->comment('帳號狀態 0:未認證,1:正常,2:lock(密碼輸入錯六次, 系統自動變更),9:block');
                $table->tinyInteger('sys_lock')->default(0)->comment('系統鎖定; 1:鎖定, 0:非解鎖');
                $table->dateTime('last_login')->nullable()->comment('最後登入時間');
                $table->bigInteger('last_login_ip')->default(0)->comment('最後登入IP');
                $table->dateTime('first_deposit')->nullable()->comment('首次儲值時間');
                $table->dateTime('first_withdrawal')->nullable()->comment('首次提領時間');
                $table->integer('total_login_fail_count')->default(0)->comment('總登入錯誤次數');
                $table->integer('login_fail_count')->default(0)->comment('登入錯誤次數');
                $table->integer('forget_pwd_count')->default(0)->comment('忘記密碼次數');
                $table->text('memo')->collation('utf8mb4_unicode_ci')->comment('方便技術記錄用');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //unique
                $table->unique('username', 'uni_' . $this->table_name . '_' . 'username');
                $table->unique('uid', 'idx_' . $this->table_name . '_' . 'uid');
                //index
                $table->index('agent_id', 'idx_' . $this->table_name . '_' . 'parent');
                $table->index('mobile', 'idx_' . $this->table_name . '_' . 'mobile');
                $table->index('email', 'idx_' . $this->table_name . '_' . 'email');
                $table->index('nickname', 'idx_' . $this->table_name . '_' . 'nickname');
                $table->index('referral_code', 'idx_' . $this->table_name . '_' . 'invite_code');
                $table->index('is_validate', 'idx_' . $this->table_name . '_' . 'is_validate');
                $table->index('mobile_validate', 'idx_' . $this->table_name . '_' . 'mobile_validate');
                $table->index('email_validate', 'idx_' . $this->table_name . '_' . 'email_validate');
                $table->index('last_login', 'idx_' . $this->table_name . '_' . 'last_login');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
                $table->index('created_at', 'idx_' . $this->table_name . '_' . 'created_at');
            });

            DB::table($this->table_name)->insert([
                ['type' => 9, 'uid' => 743985, 'agent_id' => 100001, 'username' => 'Registrants_RD', 'nickname' => 'Registrants', 'level' => 1, 'password' => 'DD203BC58AD6DAA5B0A2B4CD619C38C6', 'invitation_code' => '-', 'is_validate' => 1, 'email_validate' => 1, 'mobile_validate' => 1, 'status' => 1,],
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
