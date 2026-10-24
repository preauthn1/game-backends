<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'member_miner_temps';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->dateTime('summary_time')->comment('時間');
                $table->dateTime('data_time')->comment('數據時間');
                $table->integer('member_id')->comment('會員系統編號(members.id)');
                $table->integer('parent_id')->default(0)->comment('上級會員系統編號(members.id)');
                $table->integer('miner_id')->comment('礦機系統編號(miners.id)');
                $table->tinyInteger('type')->comment('返水類型; 0:Rental, 1:purchase, 2:group');
                $table->decimal('self_count', 6, 1)->default(0.0)->comment('自身總計');
                $table->decimal('self_amount', 30, 10)->default(0.0000000000)->comment('自身購買金額');
                $table->decimal('direct_count', 6, 1)->default(0.0)->comment('直屬總計');
                $table->decimal('direct_amount', 30, 10)->default(0.0000000000)->comment('直屬總計');
                $table->text('direct_info')->collation('utf8mb4_unicode_ci')->comment('直屬相關資訊');
                $table->decimal('team_count', 6, 1)->default(0.0)->comment('團隊總計');
                $table->decimal('team_amount', 30, 10)->default(0.0000000000)->comment('團隊總計');
                $table->text('team_info')->collation('utf8mb4_unicode_ci')->comment('直屬相關資訊');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //unique
                $table->unique(['member_id', 'miner_id', 'summary_time', 'data_time', 'type'], 'uni_member_miner_temps_1');
                //index
                $table->index('summary_time', 'idx_' . $this->table_name . '_' . 'summary_time');
                $table->index('data_time', 'idx_' . $this->table_name . '_' . 'data_time');
                $table->index('member_id', 'idx_' . $this->table_name . '_' . 'member_id');
                $table->index('miner_id', 'idx_' . $this->table_name . '_' . 'miner_id');
            });
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
