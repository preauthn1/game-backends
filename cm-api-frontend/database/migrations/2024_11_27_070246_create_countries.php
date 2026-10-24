<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'countries';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('系統編號');
                $table->string('code', 10)->collation('utf8mb4_unicode_ci')->comment('國家代碼(ISO_3166-1)');
                $table->string('name', 32)->collation('utf8mb4_unicode_ci')->comment('國家名稱');
                $table->integer('country_code')->default(0)->comment('國碼');
                $table->string('phone_rule', 191)->collation('utf8mb4_unicode_ci')->default(0)->comment('電話號碼規則');
                $table->tinyInteger('status')->default(0)->comment('狀態 0:停用,1:啟用');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');
            });

            DB::table($this->table_name)->insert([
                ['code' => 'MY', 'name' => 'Malaysia', 'country_code' => 60, 'phone_rule' => '^[1-9]\d{8,9}$', 'status' => 1],
                ['code' => 'SG', 'name' => 'Singapore', 'country_code' => 65, 'phone_rule' => '^[1-9]\d{7}$', 'status' => 0],
                ['code' => 'CN', 'name' => 'China', 'country_code' => 86, 'phone_rule' => '^[1-9]\d{10}$', 'status' => 0],
                ['code' => 'TW', 'name' => 'Taiwan', 'country_code' => 886, 'phone_rule' => '^[1-9]\d{8}$', 'status' => 0],
                ['code' => 'TH', 'name' => 'Thailand', 'country_code' => 66, 'phone_rule' => '^[1-9]\d{8}$', 'status' => 0],
                ['code' => 'LA', 'name' => 'Laos', 'country_code' => 856, 'phone_rule' => '^[1-9]\d{10}$', 'status' => 0],
                ['code' => 'ZA', 'name' => 'South Africa', 'country_code' => 27, 'phone_rule' => '^[1-9]\d{10}$', 'status' => 0],
                ['code' => 'BN', 'name' => 'Brunei', 'country_code' => 673, 'phone_rule' => '^[0-9]\d{6}$', 'status' => 0],
                ['code' => 'AU', 'name' => 'Australia', 'country_code' => 61, 'phone_rule' => '^[1-9]\d{10}$', 'status' => 0],
                ['code' => 'VN', 'name' => 'Vietnam', 'country_code' => 84, 'phone_rule' => '^[1-9]\d{8,9}$', 'status' => 0],
                ['code' => 'PH', 'name' => 'Philippines', 'country_code' => 63, 'phone_rule' => '^[1-9]\d{9}$', 'status' => 0],
                ['code' => 'HK', 'name' => 'Hong Kong', 'country_code' => 852, 'phone_rule' => '^[1-9]\d{7}$', 'status' => 0],
                ['code' => 'ID', 'name' => 'Indonesia', 'country_code' => 62, 'phone_rule' => '^[15689]\d{10,13}$', 'status' => 0],
                ['code' => 'MM', 'name' => 'Myanmar', 'country_code' => 95, 'phone_rule' => '^[1-9]\d{9}$', 'status' => 0],
                ['code' => 'IN', 'name' => 'India', 'country_code' => 91, 'phone_rule' => '^[1-9]\d{9}$', 'status' => 0],
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
