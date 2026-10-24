<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'sms_gateway_countries';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id');
                $table->integer('sms_gateway_id')->comment('簡訊商ID; sms_gateways.id');
                $table->integer('country_id')->comment('國家ID; countries.id');
                $table->tinyInteger('status')->default(0)->comment('狀態; 1:啟用, 0:停用');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('sms_gateway_id', 'idx_' . $this->table_name . '_' . 'sms_gateway_id');
                $table->index('country_id', 'idx_' . $this->table_name . '_' . 'country_id');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
            });

            DB::table($this->table_name)->insert([
                ['id' => 1, 'sms_gateway_id' => 1, 'country_id' => 1, 'status' => 1],
                ['id' => 2, 'sms_gateway_id' => 1, 'country_id' => 2, 'status' => 1],
                ['id' => 3, 'sms_gateway_id' => 1, 'country_id' => 3, 'status' => 1],
                ['id' => 4, 'sms_gateway_id' => 1, 'country_id' => 4, 'status' => 1],
                ['id' => 5, 'sms_gateway_id' => 1, 'country_id' => 5, 'status' => 1],
                ['id' => 6, 'sms_gateway_id' => 1, 'country_id' => 6, 'status' => 1],
                ['id' => 7, 'sms_gateway_id' => 1, 'country_id' => 7, 'status' => 1],
                ['id' => 8, 'sms_gateway_id' => 1, 'country_id' => 8, 'status' => 1],
                ['id' => 9, 'sms_gateway_id' => 1, 'country_id' => 9, 'status' => 1],
                ['id' => 10, 'sms_gateway_id' => 1, 'country_id' => 10, 'status' => 1],
                ['id' => 11, 'sms_gateway_id' => 1, 'country_id' => 11, 'status' => 1],
                ['id' => 12, 'sms_gateway_id' => 1, 'country_id' => 12, 'status' => 1],
                ['id' => 13, 'sms_gateway_id' => 1, 'country_id' => 13, 'status' => 1],
                ['id' => 14, 'sms_gateway_id' => 1, 'country_id' => 14, 'status' => 1],
                ['id' => 15, 'sms_gateway_id' => 1, 'country_id' => 15, 'status' => 1],
                ['id' => 16, 'sms_gateway_id' => 2, 'country_id' => 1, 'status' => 1],
                ['id' => 17, 'sms_gateway_id' => 2, 'country_id' => 2, 'status' => 0],
                ['id' => 18, 'sms_gateway_id' => 2, 'country_id' => 3, 'status' => 0],
                ['id' => 19, 'sms_gateway_id' => 2, 'country_id' => 4, 'status' => 0],
                ['id' => 20, 'sms_gateway_id' => 2, 'country_id' => 5, 'status' => 0],
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
