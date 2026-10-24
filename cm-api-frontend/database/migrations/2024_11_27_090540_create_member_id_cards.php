<?php

use Illuminate\Database\Migrations\Migration;
use Illuminate\Database\Schema\Blueprint;
use Illuminate\Support\Facades\Schema;
use Illuminate\Support\Facades\DB;

return new class extends Migration {
    protected string $table_name = 'member_id_cards';

    /**
     * Run the migrations.
     */
    public function up(): void
    {
        if (!Schema::hasTable($this->table_name)) {
            Schema::create($this->table_name, function (Blueprint $table) {
                $table->increments('id')->comment('流水號');
                $table->integer('member_id')->default(1)->comment('會員系統編號(members.id)');
                $table->text('image_front')->collation('utf8mb4_unicode_ci')->comment('ID FRONT');
                $table->text('image_back')->collation('utf8mb4_unicode_ci')->comment('ID BACK');
                $table->tinyInteger('status')->default(0)->comment('狀態: 0:待審核, 1:已實名, 2:拒絕');
                $table->text('remark')->collation('utf8mb4_unicode_ci')->comment('備註');
                $table->integer('operator')->default(0)->comment('操作人');
                $table->timestamp('created_at')
                    ->useCurrent()
                    ->comment('建立日期');
                $table->timestamp('updated_at')
                    ->default(DB::raw('CURRENT_TIMESTAMP on update CURRENT_TIMESTAMP'))
                    ->comment('更新日期');

                //index
                $table->index('member_id', 'idx_' . $this->table_name . '_' . 'member_id');
                $table->index('status', 'idx_' . $this->table_name . '_' . 'status');
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
