import 'package:collection/collection.dart';
import 'package:flutter/material.dart';

import '../models/miner_group_order.dart';
import '../models/miner_rebate.dart';
import '../models/miner_referral_reward.dart';
import '../models/my_miner.dart';
import '../models/my_miner_detail.dart';
import '../models/purchase_miner.dart';
import '../models/rent_miner.dart';
import '../utils/http_service.dart';

class MinerProvider with ChangeNotifier {
  List<PurchaseMiner> _purchaseMiners = [];
  List<MyMiner> _myPurchaseMiners = [];
  List<RentMiner> _rentMiners = [];
  List<RentMiner> _rentList = [];
  MyMinerDetail? _myMinerDetail;
  List<MyMiner> _myMinerRentMiners = [];
  List<PurchaseMiner> _groupMiners = [];
  List<MinerGroupOrder> _groupOrders = [];
  List<MinerRebate> _rebates = [];
  List<MinerReferralReward> _referralRewards = [];

  List<PurchaseMiner> get purchaseMiners => _purchaseMiners;
  List<MyMiner> get myPurchaseMiners => _myPurchaseMiners;
  List<RentMiner> get rentMiners => _rentMiners;
  List<RentMiner> get rentList => _rentList;
  MyMinerDetail? get myMinerDetail => _myMinerDetail;
  List<MyMiner> get myMinerRentMiners => _myMinerRentMiners;
  List<PurchaseMiner> get groupMiners => _groupMiners;
  List<MinerGroupOrder> get groupOrders => _groupOrders;
  List<MinerRebate> get rebates => _rebates;
  List<MinerReferralReward> get referralRewards => _referralRewards;

  // 取得購買礦機
  PurchaseMiner? getPurchaseMinerById(String id) {
    return _purchaseMiners.firstWhereOrNull((miner) => miner.id == id);
  }

  // 取得租賃礦機
  RentMiner? getRentMinerByMmId(String mmId) {
    return _rentList.firstWhereOrNull((miner) => miner.mmId == mmId);
  }

  // 取得團購礦機
  PurchaseMiner? getGroupMinerById(String id) {
    return _groupMiners.firstWhereOrNull((miner) => miner.id == id);
  }

  // 取得團購礦機合併單
  MinerGroupOrder? getGroupOrderById(String mmId) {
    return _groupOrders.firstWhereOrNull((order) => order.mmId == mmId);
  }

  void clear() {
    _purchaseMiners = [];
    _myPurchaseMiners = [];
    _rentMiners = [];
    _rentList = [];
    _myMinerDetail = null;
    _myMinerRentMiners = [];
    _groupMiners = [];
    _groupOrders = [];
    _rebates = [];
    _referralRewards = [];
    notifyListeners();
  }

  // 取得購買礦機列表
  Future<void> fetchPurchaseMiners() async {
    final res = await HttpService.post(path: 'api/miner/getPurchase');
    _purchaseMiners = List<PurchaseMiner>.from(
      res['data']['list'].map((item) => PurchaseMiner.fromJson(item)),
    );
    notifyListeners();
  }

  // 取得我的礦機列表
  Future<void> fetchMyPurchaseMiners() async {
    final res = await HttpService.post(path: 'api/member/getMyMiners', data: {
      'miner_type': 1,
    });
    _myPurchaseMiners = List<MyMiner>.from(
      res['data']['list'].map((item) => MyMiner.fromJson(item)),
    );
    notifyListeners();
  }

  // 取得我的礦機詳細資訊
  Future<void> fetchMyMinerDetail(int mmId) async {
    final res =
        await HttpService.post(path: 'api/member/getMyMinerDetail', data: {
      'mm_id': mmId,
    });
    _myMinerDetail = MyMinerDetail.fromJson(res['data']['list']);
  }

  // 取得我的礦機殘值
  Future<String?> fetchMyMinerBalance(String mmId) async {
    final res = await HttpService.post(path: 'api/miner/getRemain', data: {
      'mm_id': mmId,
    });
    return res['data']['residual_value'];
  }

  // 礦機殘值試算
  Future<String?> fetchTrialDepreciation({
    required String mmId,
    required DateTime date,
  }) async {
    final dateFormatted =
        date.toIso8601String().substring(0, 10).replaceAll('-', '/');
    final res = await HttpService.post(
      path: 'api/miner/getTrailRemain',
      data: {
        'mm_id': mmId,
        'calculate_date': dateFormatted,
      },
    );
    return res['data']['residual_value'];
  }

  // 購買礦機
  Future<void> purchaseMiner({
    required String minerId,
    required int quantity,
  }) async {
    await HttpService.post(
      path: "api/miner/doPurchase",
      data: {"miner_id": minerId, "quantity": quantity},
    );
  }

  //出售礦機
  Future<void> sellBackMiner(String mmId) async {
    await HttpService.post(
      path: "api/miner/sellBackMiner",
      data: {"mm_id": mmId},
    );
  }

  //出售礦機
  Future<void> sellBackGroupMiner(String mmId) async {
    await HttpService.post(
      path: "api/miner/sellBackGroupMiner",
      data: {"mm_id": mmId},
    );
  }

  // 取得全部租賃礦機
  Future<void> fetchRentMiners() async {
    final res = await HttpService.post(path: 'api/miner/getRentAll');
    _rentMiners = List<RentMiner>.from(
      res['data']['list'].map((item) => RentMiner.fromJson(item)),
    );
    notifyListeners();
  }

  // 取得租賃礦機列表
  Future<void> fetchRentList(int minerId) async {
    final res = await HttpService.post(path: 'api/miner/getRentList', data: {
      'miner_id': minerId,
    });
    _rentList = List<RentMiner>.from(
      res['data']['list'].map((item) => RentMiner.fromJson(item)),
    );
    notifyListeners();
  }

  // 租賃礦機
  Future<void> doRentMiners(List<String> mmIds) async {
    await HttpService.post(
      path: "api/miner/doRent",
      data: {"mm_ids": mmIds},
    );
  }

  // 取得我的租賃礦機列表
  Future<void> fetchMyMinerRentMiners() async {
    final res = await HttpService.post(path: 'api/member/getMyMiners', data: {
      'miner_type': 0,
    });
    _myMinerRentMiners = List<MyMiner>.from(
      res['data']['list'].map((item) => MyMiner.fromJson(item)),
    );
    notifyListeners();
  }

  // 取得團購礦機列表
  Future<void> fetchGroupMiners() async {
    final res = await HttpService.post(path: 'api/miner/getGroup');
    _groupMiners = List<PurchaseMiner>.from(
      res['data']['list'].map((item) => PurchaseMiner.fromJson(item)),
    );
    notifyListeners();
  }

  // 取得團購礦機合併單列表
  Future<void> fetchGroupOrders(int minerId) async {
    final res = await HttpService.post(
      path: 'api/miner/getGroupList',
      data: {'miner_id': minerId},
    );
    _groupOrders = List<MinerGroupOrder>.from(
      res['data']['list'].map((item) => MinerGroupOrder.fromJson(item)),
    );

    notifyListeners();
  }

  // 購買團購礦機
  Future<void> purchaseGroupMiner(String mmId) async {
    await HttpService.post(
      path: "api/miner/doGroup",
      data: {"mm_id": mmId},
    );
  }

  // 創建團購礦機
  Future<void> createGroupMiner(String minerId) async {
    await HttpService.post(
      path: "api/miner/createGroup",
      data: {"miner_id": minerId},
    );
  }

  // 取消團購礦機
  Future<void> cancelGroupMiner(String mmId) async {
    await HttpService.post(
      path: "api/miner/cancelGroup",
      data: {"mm_id": mmId},
    );
  }

  // 取得返利列表
  Future<void> fetchRebateList() async {
    final res = await HttpService.post(path: 'api/miner/getRebateList');
    _rebates = List<MinerRebate>.from(
      res['data']['list'].map((item) => MinerRebate.fromJson(item)),
    );
    notifyListeners();
  }

  // 取得推薦獎勵列表
  Future<void> fetchReferralRewardList() async {
    final res = await HttpService.post(path: 'api/miner/getReferralRewardList');
    _referralRewards = List<MinerReferralReward>.from(
      res['data']['list'].map((item) => MinerReferralReward.fromJson(item)),
    );
    notifyListeners();
  }
}
