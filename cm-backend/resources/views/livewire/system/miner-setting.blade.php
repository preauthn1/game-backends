@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.system.minerSetting')}}</h1>
    </div>
    <div id="page-content">
        <!--搜索區塊-->
        <div class="panel">
            <div class="panel-heading">
                <h3 class="panel-title">{{__('web_message.general.search')}}</h3>
            </div>
            <div class="panel-body">
                <form class="form-inline" wire:submit="doSearch" style="position: relative;">
                    <div class="form-group">
                        <label>{{__('web_message.general.minerName')}}：</label>
                        <input type="text" class="form-control" wire:model="minerName">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.menuMgmt.isShow')}}：</label>
                        <select class="form-select form-control" wire:model="minerDisplay">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            <option value="1">{{__('web_message.display.show')}}</option>
                            <option value="0">{{__('web_message.display.hide')}}</option>

                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.status')}}：</label>
                        <select class="form-select form-control" wire:model="minerStatus">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            <option value="1">{{__('web_message.general.statusList.enable')}}</option>
                            <option value="0">{{__('web_message.general.statusList.disable')}}</option>

                        </select>
                    </div>
                    <button class="btn btn-primary btn-search"
                            type="submit">{{__('web_message.general.search')}}</button>
                </form>
            </div>
        </div>
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="pad-btm form-inline">
                    <div class="row">
                        <div class="col-sm-6 table-toolbar-left">
                            <button class="btn btn-purple" data-toggle="modal" data-target="#addMiner">
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
                            <th>{{__('web_message.general.status')}}</th>
                            <th>{{__('web_message.general.minerName')}}</th>
                            <th>{{__('web_message.general.miningCurrency')}}</th>
                            <th>{{__('web_message.general.dayRevenue')}}(USDT)</th>
                            <th>{{__('web_message.general.depreciationCycle')}}</th>
                            <th>{{__('web_message.general.purchasePrice')}}(USDT)</th>
                            <th>{{__('web_message.general.purchaseTotal')}}</th>
                            <th>{{__('web_message.general.rentPrice')}}(USDT)</th>
                            <th>{{__('web_message.menuMgmt.order')}}</th>
                            <th>{{__('web_message.menuMgmt.isShow')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $data)
                            <tr>
                                <td>
                                    <button class="btn btn-warning"
                                            wire:click="showDataPop({{$data->miner_id}},'modify')">
                                        <i class="bi-pencil-square"></i>
                                    </button>
                                    <button class="btn btn-danger"
                                            wire:click="showDataPop({{$data->miner_id}},'detail')">
                                        <i class="bi-list-task"></i>
                                    </button>
                                </td>
                                <td style="font-size: 16px;">
                                    <i class="{{(1==$data->status)?'bi-toggle-on':'bi-toggle-off'}}"></i>
                                </td>
                                <td>{{$data->name}}</td>
                                <td>{{$data->currency_name}}</td>
                                <td>{{number_format($data->day_revenue,session()->get('defaultScale'))}}</td>
                                <td>{{$data->depreciation_cycle}}</td>
                                <td>{{number_format($data->purchase_price,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->purchase_total,session()->get('defaultScale'))}}</td>
                                <td>{{number_format($data->rent_price,session()->get('defaultScale'))}}</td>
                                <td>{{ $data->sort }}</td>
                                <td style="font-size: 16px;"><i
                                        class="{{(1==$data->is_show)?'bi-toggle-on':'bi-toggle-off'}}"></i></td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="11">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="11" class="footable-visible">
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
        <!-- addMiner -->
        <div class="modal fade" id="addMiner" role="dialog">
            <div class="modal-dialog" style="margin-top: 30px;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal" id="addMinerClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.system.addMiner')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addMiner">
                            <div class="panel-body" style="padding: 0;">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.minerName')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addMinerName">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.minerImage')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <ul class="mail-attach-list list-ov">
                                            <li>
                                                <div class="mail-file-img" style="border: 1px solid #ddd;">
                                                    <img class="image-responsive" id="addPreviewImage"
                                                         style="width: 120px;">
                                                </div>
                                            </li>
                                        </ul>
                                        <input type="file" class="form-control" accept="image/*" id="addSelectImage">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.dayRevenue')}}(USDT)
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addMinerDayRevenue"
                                               oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d\d\d).*$/,'$1.$2')">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.menuMgmt.order')}}
                                    </label>
                                    <div class="col-sm-2">
                                        <input type="text" class="form-control" wire:model="addMinerOrder"
                                               oninput="value=value.replace(/\D/g,'')">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.menuMgmt.isShow')}}
                                    </label>
                                    <div class="col-sm-4 lh-30">
                                        <input id="add-miner-display" class="toggle-switch" type="checkbox"
                                               wire:model="addMinerDisplay">
                                        <label for="add-miner-display"></label>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        <span class="required">*</span>{{__('web_message.general.status')}}
                                    </label>
                                    <div class="col-sm-4 lh-30">
                                        <input id="add-miner-status" class="toggle-switch" type="checkbox"
                                               wire:model="addMinerStatus">
                                        <label for="add-miner-status"></label>
                                    </div>
                                </div>
                                <div class="panel-group accordion">
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                   href="#basic">{{__('web_message.general.basic')}}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse in" id="basic">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.miningCurrency')}}
                                                    </label>
                                                    <div class="col-sm-5">
                                                        <select class="form-select form-control"
                                                                wire:model="addMiningCurrency">
                                                            <option value="0"
                                                                    selected>{{__('web_message.general.select')}}{{__('web_message.general.currency')}}</option>
                                                            @forelse((session()->get('pageCacheData')['currencyList']??[]) as $data)
                                                                <option
                                                                    value="{{$data->currency_id}}">{{$data->currency_name}}</option>
                                                            @empty
                                                            @endforelse
                                                        </select>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.depreciationCycle')}}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/\D/g,'')"
                                                               wire:model="addMinerDepreciationCycle">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.hashRate')}}
                                                    </label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/\D/g,'')"
                                                               wire:model="addMinerHashRate">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.powerConsumption')}}
                                                    </label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                               wire:model="addMinerPowerConsumption">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.dayElecFee')}}
                                                    </label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                               wire:model="addMinerDayElecFee">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.custodyFee')}}
                                                    </label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                               wire:model="addMinerCustodyFee">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.custodyFeeRate')}}
                                                    </label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                               wire:model="addMinerCustodyFeeRate">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        {{__('web_message.general.tagTxt')}}
                                                    </label>
                                                    <div class="col-sm-5">
                                                        <input type="text" class="form-control"
                                                               wire:model="addMinerTagTxt">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        {{__('web_message.general.tagColor')}}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="color" class="form-control" style="height: 35px;"
                                                               wire:model="addMinerTagColor">
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                   href="#purchase">{{__('web_message.general.purchase')}}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse" id="purchase">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.purchasePrice')}}
                                                    </label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               wire:model="addMinerPurchasePrice"
                                                               oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.purchaseTotal')}}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/\D/g,'')"
                                                               wire:model="addMinerPurchaseTotal">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.purchaseRentCount')}}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/\D/g,'')"
                                                               wire:model="addMinerPurchaseRentCount">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.purchaseRentCycle')}}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/\D/g,'')"
                                                               wire:model="addMinerPurchaseRentCycle">
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                   href="#rent">{{__('web_message.general.rent')}}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse" id="rent">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label class="col-sm-6 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.rentPrice')}}
                                                    </label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                               wire:model="addMinerRentPrice">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-6 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.experienceCommission')}}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                               wire:model="addMinerExperienceCommission">
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                   href="#group">{{__('web_message.general.group')}}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse" id="group">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.groupTotal')}}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/\D/g,'')"
                                                               wire:model="addMinerGroupTotal">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.groupRentCount')}}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/\D/g,'')"
                                                               wire:model="addMinerGroupRentCount">
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{__('web_message.general.groupRentCycle')}}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/\D/g,'')"
                                                               wire:model="addMinerGroupRentCycle">
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                    href="#rebate">{{ __('web_message.general.rebate') }}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse" id="rebate">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{ __('param.member_child.level.0') }}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                            oninput="value = value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                            wire:model="addMinerRebatePercentage1">
                                                    </div>
                                                    <label class="control-label">%</label>
                                                </div>

                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{ __('param.member_child.level.1') }}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                            oninput="value = value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                            wire:model="addMinerRebatePercentage2">
                                                    </div>
                                                    <label class="control-label">%</label>
                                                </div>

                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>{{ __('param.member_child.level.2') }}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                            oninput="value = value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                            wire:model="addMinerRebatePercentage3">
                                                    </div>
                                                    <label class="control-label">%</label>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                    href="#reward">{{ __('web_message.general.reward') }}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse" id="reward">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>3 {{ __('web_message.general.member') }}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                            oninput="value = value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                            wire:model="addMinerRewardQuantity3">
                                                    </div>
                                                </div>

                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>5 {{ __('web_message.general.member') }}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                            oninput="value = value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                            wire:model="addMinerRewardQuantity5">
                                                    </div>
                                                </div>

                                                <div class="form-group">
                                                    <label class="col-sm-4 control-label">
                                                        <span
                                                            class="required">*</span>10 {{ __('web_message.general.member') }}
                                                    </label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                            oninput="value = value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                            wire:model="addMinerRewardQuantity10">
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
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
        <!-- minerPop -->
        <div class="modal fade in" id="minerPop" role="dialog"
             style="overflow-y: auto; display: {{'minerPop' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="margin-top: 30px; z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="minerPopClose">&times;</button>
                        <h4 class="modal-title">{{'modify'==(session()->get('pageCacheData')['popAction']??'')?__('web_message.menu.system.modifyMiner'):__('web_message.menu.system.MinerDetail')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="doModify">
                            <div class="panel-body" style="padding: 0;">
                                <input type="hidden" wire:model="popMinerId">
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.minerName')}}</label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="popMinerName" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.minerImage')}}</label>
                                    <div class="col-sm-6">
                                        <ul class="mail-attach-list list-ov">
                                            <li>
                                                <div class="mail-file-img" style="border: 1px solid #ddd;">
                                                    <img class="image-responsive" src="{{$popMinerImageUrl}}"
                                                         style="width: 120px;" id="popPreviewImage">
                                                </div>
                                            </li>
                                        </ul>
                                        <input type="file" class="form-control" id="popSelectImage"
                                               accept="image/*"
                                               style="display: {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'block':'none'}};">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">
                                        {{__('web_message.general.dayRevenue')}}(USDT)
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control"
                                               wire:model="popMinerDayRevenue"
                                               oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d\d\d).*$/,'$1.$2')"
                                            {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.menuMgmt.order')}}</label>
                                    <div class="col-sm-4 lh-30">
                                        <input type="text" class="form-control" oninput="value=value.replace(/\D/g,'')"
                                               wire:model="popMinerOrder" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.menuMgmt.isShow')}}</label>
                                    <div class="col-sm-4 lh-30">
                                        <input id="pop-miner-display" class="toggle-switch" type="checkbox"
                                               wire:model="popMinerDisplay" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                        <label for="pop-miner-display"></label>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.status')}}</label>
                                    <div class="col-sm-4 lh-30">
                                        <input id="pop-miner-status" class="toggle-switch" type="checkbox"
                                               wire:model="popMinerStatus" disabled>
                                        <label for="pop-miner-status"></label>
                                    </div>
                                </div>
                                <div class="panel-group accordion">
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                   href="#popBasic">{{__('web_message.general.basic')}}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse in" id="popBasic">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.miningCurrency')}}</label>
                                                    <div class="col-sm-4">
                                                        <select class="form-select form-control"
                                                                wire:model="popMiningCurrency" disabled>
                                                            <option value="0"
                                                                    selected>{{__('web_message.general.select')}}{{__('web_message.general.currency')}}</option>
                                                            @forelse((session()->get('pageCacheData')['currencyList']??[]) as $data)
                                                                <option
                                                                    value="{{$data->currency_id}}">{{$data->currency_name}}</option>
                                                            @empty
                                                            @endforelse
                                                        </select>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.depreciationCycle')}}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerDepreciationCycle" disabled>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.hashRate')}}</label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerHashRate" disabled>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.powerConsumption')}}</label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerPowerConsumption" disabled>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.dayElecFee')}}</label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerDayElecFee" disabled>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.custodyFee')}}</label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerCustodyFee" disabled>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.custodyFeeRate')}}</label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerCustodyFeeRate" disabled>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.tagTxt')}}</label>
                                                    <div class="col-sm-5">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerTagTxt" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.tagColor')}}</label>
                                                    <div class="col-sm-3">
                                                        <input type="color" class="form-control" style="height: 35px;"
                                                               wire:model="popMinerTagColor" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                   href="#popPurchase">{{__('web_message.general.purchase')}}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse" id="popPurchase">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.purchasePrice')}}</label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerPurchasePrice" disabled>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.purchaseTotal')}}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/\D/g,'')"
                                                               wire:model="popMinerPurchaseTotal" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.purchaseRentCount')}}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerPurchaseRentCount" disabled>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.purchaseRentCycle')}}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerPurchaseRentCycle" disabled>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                   href="#popRent">{{__('web_message.general.rent')}}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse" id="popRent">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-6 control-label">{{__('web_message.general.rentPrice')}}</label>
                                                    <div class="col-sm-4">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerRentPrice" disabled>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-6 control-label">{{__('web_message.general.experienceCommission')}}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control" wire:model="popMinerExperienceCommission"
                                                               oninput="value = value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                               {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                   href="#popGroup">{{__('web_message.general.group')}}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse" id="popGroup">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.groupTotal')}}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value=value.replace(/\D/g,'')"
                                                               wire:model="popMinerGroupTotal" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.groupRentCount')}}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerGroupRentCount" disabled>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('web_message.general.groupRentCycle')}}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerGroupRentCycle" disabled>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                   href="#popRebate">{{__('web_message.general.rebate')}}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse" id="popRebate">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('param.member_child.level.0')}}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value = value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                               wire:model="popMinerRebatePercentage1" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                    </div>
                                                    <label class="control-label">%</label>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('param.member_child.level.1')}}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value = value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                               wire:model="popMinerRebatePercentage2" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                    </div>
                                                    <label class="control-label">%</label>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">{{__('param.member_child.level.2')}}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               oninput="value = value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                                               wire:model="popMinerRebatePercentage3" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                    </div>
                                                    <label class="control-label">%</label>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="panel">
                                        <div class="panel-heading">
                                            <h4 class="panel-title">
                                                <a data-parent="#accordion" data-toggle="collapse"
                                                   href="#popReward">{{__('web_message.general.reward')}}</a>
                                            </h4>
                                        </div>
                                        <div class="panel-collapse collapse" id="popReward">
                                            <div class="panel-body">
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">3 {{ __('web_message.general.member') }}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerRewardQuantity3" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">5 {{ __('web_message.general.member') }}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerRewardQuantity5" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                    </div>
                                                </div>
                                                <div class="form-group">
                                                    <label
                                                        class="col-sm-4 control-label">10 {{ __('web_message.general.member') }}</label>
                                                    <div class="col-sm-3">
                                                        <input type="text" class="form-control"
                                                               wire:model="popMinerRewardQuantity10" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                            </div>
                            @if('modify'==(session()->get('pageCacheData')['popAction']??''))
                                <div class="panel-footer">
                                    <div class="row">
                                        <div class="col-sm-offset-10">
                                            <button class="btn btn-mint"
                                                    type="submit">{{__('web_message.general.modify')}}</button>
                                        </div>
                                    </div>
                                </div>
                            @endif
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!--頁面特殊JS-->
        <script>
            $(function () {
                $('#addMinerClose').click(function () {
                    $('#addSelectImage').val('');
                    $('#addPreviewImage').attr('src', '');
                });
                $('#minerPopClose').click(function () {
                    $('#minerPop').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                    $('#popSelectImage').val('');
                });

                function addMinerReadFile() {
                    if (!this.files || !this.files[0]) return;
                    const FR = new FileReader();
                    FR.addEventListener("load", function (evt) {
                        document.querySelector("#addPreviewImage").src = evt.target.result;
                        Livewire.dispatch('addMinerImage', {data: evt.target.result});
                        //document.querySelector("#base64Image").textContent = evt.target.result;
                    });
                    FR.readAsDataURL(this.files[0]);
                }

                document.querySelector("#addSelectImage").addEventListener("change", addMinerReadFile);

                function popMinerReadFile() {
                    if (!this.files || !this.files[0]) return;
                    const FR = new FileReader();
                    FR.addEventListener("load", function (evt) {
                        document.querySelector("#popPreviewImage").src = evt.target.result;
                        Livewire.dispatch('popMinerImage', {data: evt.target.result});
                        //document.querySelector("#base64Image").textContent = evt.target.result;
                    });
                    FR.readAsDataURL(this.files[0]);
                }

                document.querySelector("#popSelectImage").addEventListener("change", popMinerReadFile);
            });
        </script>
    </div>
@endsection
