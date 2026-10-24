@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.system.rankingList')}}</h1>
    </div>
    <div id="page-content">
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="pad-btm form-inline">
                    <div class="row">
                        <div class="col-sm-6 table-toolbar-left">
                            <button class="btn btn-purple" data-toggle="modal" data-target="#addRankList">
                                <i class="bi-plus-circle"></i> {{__('web_message.general.add')}}
                            </button>
                        </div>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            <th></th>
                            <th>{{__('web_message.general.seq')}}</th>
                            <th>UID</th>
                            <th>{{__('web_message.general.username')}}</th>
                            <th>{{__('web_message.general.quantitySold')}}</th>
                            <th>{{__('web_message.general.rebateAmount')}}(USDT)</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $seq=>$data)
                            <tr>
                                <td>
                                    <button class="btn btn-warning" wire:click="showModify({{$data->id}})"
                                            id="showModify">
                                        <i class="bi-pencil-square"></i>
                                    </button>
                                    <button class="btn btn-danger" wire:click="doDelete({{$data->id}})">
                                        <i class="bi-trash3"></i>
                                    </button>
                                </td>
                                <td>{{(session()->get('nowPage')-1)*10+($seq+1)}}</td>
                                <td>{{$data->uid}}</td>
                                <td>{{$data->username}}</td>
                                <td>{{number_format($data->quantity_sold)}}</td>
                                <td>{{number_format($data->rebate_amount,session()->get('defaultScale'))}}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="6">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="6" class="footable-visible">
                                <div class="text-right" style="position: relative;">
                                    <div class="total-entries">
                                        {{(session()->get('pageCacheData')['totalRow']??0)}} {{__('web_message.general.entries')}}
                                    </div>
                                    <x-pagination></x-pagination>
                                </div>
                            </td>
                        </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
        </div>
        <!-- addRankList -->
        <div class="modal fade" id="addRankList" role="dialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.system.addRankList')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addRankList">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.username')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addRankListUsername">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.quantitySold')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addRankListQty"
                                               oninput="value=value.replace(/\D/g,'')">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.rebateAmount')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="addRankListRebate"
                                               oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                                    </div>
                                </div>
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                                type="submit">{{__('web_message.general.add')}}</button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!-- modifyRankList -->
        <div class="modal fade in" id="modifyRankList" role="dialog"
             style="display: {{'modifyRankList' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="modifyClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.system.modifyRankList')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="doModify">
                            <div class="panel-body">
                                <input type="hidden" wire:model="modifyRankListId">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.general.username')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="modifyRankListUsername"
                                               disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.general.quantitySold')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="modifyRankListQty"
                                               oninput="value=value.replace(/\D/g,'')">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.general.rebateAmount')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="modifyRankListRebate"
                                               oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                                    </div>
                                </div>
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                                type="submit">{{__('web_message.general.modify')}}</button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!--頁面特殊JS-->
        <script>
            $(function () {
                $('#modifyClose').click(function () {
                    $('#modifyRankList').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
            });
        </script>
    </div>
@endsection
