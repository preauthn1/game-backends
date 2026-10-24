<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'sms_gateway_info_countries';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->integer('sms_gateway_info_id')->comment('簡訊商戶資訊ID; sms_gateway_infos.id');
                $table->integer('country_id')->comment('國家ID; countries.id');
                $table->tinyInteger('status')->default(0)->comment('狀態; 1:啟用, 0:停用');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('sms_gateway_info_id', 'idx_' . $this->table_name . '_' . 'sms_gateway_info_id');
                $table->index('country_id', 'idx_' . $this->table_name . '_' . 'country_id');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'sms_gateway_info_id' => 1, 'country_id' => 1, 'status' => 1],
                ['id' => 2, 'sms_gateway_info_id' => 1, 'country_id' => 2, 'status' => 0],
                ['id' => 3, 'sms_gateway_info_id' => 1, 'country_id' => 3, 'status' => 0],
                ['id' => 4, 'sms_gateway_info_id' => 1, 'country_id' => 4, 'status' => 0],
                ['id' => 5, 'sms_gateway_info_id' => 1, 'country_id' => 5, 'status' => 0],
                ['id' => 6, 'sms_gateway_info_id' => 1, 'country_id' => 6, 'status' => 0],
                ['id' => 7, 'sms_gateway_info_id' => 1, 'country_id' => 7, 'status' => 0],
                ['id' => 8, 'sms_gateway_info_id' => 1, 'country_id' => 8, 'status' => 0],
                ['id' => 9, 'sms_gateway_info_id' => 1, 'country_id' => 9, 'status' => 0],
                ['id' => 10, 'sms_gateway_info_id' => 1, 'country_id' => 10, 'status' => 0],
                ['id' => 11, 'sms_gateway_info_id' => 1, 'country_id' => 11, 'status' => 0],
                ['id' => 12, 'sms_gateway_info_id' => 1, 'country_id' => 12, 'status' => 0],
                ['id' => 13, 'sms_gateway_info_id' => 1, 'country_id' => 13, 'status' => 0],
                ['id' => 14, 'sms_gateway_info_id' => 1, 'country_id' => 14, 'status' => 0],
                ['id' => 15, 'sms_gateway_info_id' => 1, 'country_id' => 15, 'status' => 0],
                ['id' => 16, 'sms_gateway_info_id' => 2, 'country_id' => 1, 'status' => 1],
                ['id' => 17, 'sms_gateway_info_id' => 2, 'country_id' => 2, 'status' => 0],
                ['id' => 18, 'sms_gateway_info_id' => 2, 'country_id' => 3, 'status' => 0],
                ['id' => 19, 'sms_gateway_info_id' => 2, 'country_id' => 4, 'status' => 0],
                ['id' => 20, 'sms_gateway_info_id' => 2, 'country_id' => 5, 'status' => 0],
                ['id' => 21, 'sms_gateway_info_id' => 2, 'country_id' => 6, 'status' => 0],
                ['id' => 22, 'sms_gateway_info_id' => 2, 'country_id' => 7, 'status' => 0],
                ['id' => 23, 'sms_gateway_info_id' => 2, 'country_id' => 8, 'status' => 0],
                ['id' => 24, 'sms_gateway_info_id' => 2, 'country_id' => 9, 'status' => 0],
                ['id' => 25, 'sms_gateway_info_id' => 2, 'country_id' => 10, 'status' => 0],
                ['id' => 26, 'sms_gateway_info_id' => 2, 'country_id' => 11, 'status' => 0],
                ['id' => 27, 'sms_gateway_info_id' => 2, 'country_id' => 12, 'status' => 0],
                ['id' => 28, 'sms_gateway_info_id' => 2, 'country_id' => 13, 'status' => 0],
                ['id' => 29, 'sms_gateway_info_id' => 2, 'country_id' => 14, 'status' => 0],
                ['id' => 30, 'sms_gateway_info_id' => 2, 'country_id' => 15, 'status' => 0],
                ['id' => 31, 'sms_gateway_info_id' => 3, 'country_id' => 1, 'status' => 1],
                ['id' => 32, 'sms_gateway_info_id' => 3, 'country_id' => 2, 'status' => 0],
                ['id' => 33, 'sms_gateway_info_id' => 3, 'country_id' => 3, 'status' => 0],
                ['id' => 34, 'sms_gateway_info_id' => 3, 'country_id' => 4, 'status' => 0],
                ['id' => 35, 'sms_gateway_info_id' => 3, 'country_id' => 5, 'status' => 0],
                ['id' => 36, 'sms_gateway_info_id' => 3, 'country_id' => 6, 'status' => 0],
                ['id' => 37, 'sms_gateway_info_id' => 3, 'country_id' => 7, 'status' => 0],
                ['id' => 38, 'sms_gateway_info_id' => 3, 'country_id' => 8, 'status' => 0],
                ['id' => 39, 'sms_gateway_info_id' => 3, 'country_id' => 9, 'status' => 0],
                ['id' => 40, 'sms_gateway_info_id' => 3, 'country_id' => 10, 'status' => 0],
                ['id' => 41, 'sms_gateway_info_id' => 3, 'country_id' => 11, 'status' => 0],
                ['id' => 42, 'sms_gateway_info_id' => 3, 'country_id' => 12, 'status' => 0],
                ['id' => 43, 'sms_gateway_info_id' => 3, 'country_id' => 13, 'status' => 0],
                ['id' => 44, 'sms_gateway_info_id' => 3, 'country_id' => 14, 'status' => 0],
                ['id' => 45, 'sms_gateway_info_id' => 3, 'country_id' => 15, 'status' => 0],
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
