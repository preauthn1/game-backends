<?php

namespace App\Http\Controllers\Record;

use Illuminate\Http\Request;
use App\Http\Controllers\Controller;
use Exception;

class MainController extends Controller
{
    /**
     * 類型下拉選單
     */
    public function getTypeDropdown(Request $request)
    {
        try {
            $list = array();
            $list[] = array('id' => 1, 'name' => __('param.transaction_type.1'));
            $list[] = array('id' => 2, 'name' => __('param.transaction_type.2'));
            $list[] = array('id' => 3, 'name' => __('param.transaction_type.3'));
            $list[] = array('id' => 4, 'name' => __('param.transaction_type.4'));
            $list[] = array('id' => 5, 'name' => __('param.transaction_type.5'));
            $list[] = array('id' => 6, 'name' => __('param.transaction_type.6'));
            $list[] = array('id' => 7, 'name' => __('param.transaction_type.7'));
            $list[] = array('id' => 8, 'name' => __('param.transaction_type.8'));
            $list[] = array('id' => 9, 'name' => __('param.transaction_type.9'));
            $list[] = array('id' => 10, 'name' => __('param.transaction_type.10'));
            $list[] = array('id' => 11, 'name' => __('param.transaction_type.11'));
            $list[] = array('id' => 12, 'name' => __('param.transaction_type.12'));
            $list[] = array('id' => 13, 'name' => __('param.transaction_type.13'));
            $list[] = array('id' => 14, 'name' => __('param.transaction_type.14'));
            $list[] = array('id' => 15, 'name' => __('param.transaction_type.15'));
            $list[] = array('id' => 16, 'name' => __('param.transaction_type.16'));
            $list[] = array('id' => 17, 'name' => __('param.transaction_type.17'));
            $list[] = array('id' => 18, 'name' => __('param.transaction_type.18'));

            return response()->json([
                'code' => 1000,
                'message' => __('error_message.1000'),
                'data' => [
                    'list' => $list
                ]
            ]);

        } catch (Exception $exception) {
            return response()->json([
                'code' => $exception->getCode(),
                'message' => $exception->getMessage()
            ]);
        }
    }
}
