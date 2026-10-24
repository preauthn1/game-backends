import 'dart:math';
import 'package:flutter/material.dart';

import '../models/purchase_miner.dart';
import '../models/rent_miner.dart';
import './miner_item.dart';

class MinerList extends StatelessWidget {
  final double space;
  final List<PurchaseMiner>? purchaseMiners;
  final List<RentMiner>? rentMiners;
  final Function(PurchaseMiner miner)? onTapPurchase;
  final Function(RentMiner miner)? onTapRent;

  const MinerList.purchase({
    required this.purchaseMiners,
    super.key,
    this.onTapPurchase,
    this.space = 10.0,
  })  : rentMiners = null,
        onTapRent = null;

  const MinerList.rent({
    required this.rentMiners,
    super.key,
    this.onTapRent,
    this.space = 10.0,
  })  : purchaseMiners = null,
        onTapPurchase = null;

  @override
  Widget build(BuildContext context) {
    double containerWidth = min(MediaQuery.of(context).size.width, 700);
    return Wrap(
      spacing: space,
      runSpacing: space,
      children: purchaseMiners != null
          ? purchaseMiners!
              .map(
                (miner) => SizedBox(
                  width: containerWidth / 2 - space,
                  child: MinerItem.purchase(
                    miner,
                    onTapPurchase: onTapPurchase,
                  ),
                ),
              )
              .toList()
          : rentMiners != null
              ? rentMiners!
                  .map(
                    (miner) => SizedBox(
                      width: containerWidth / 2 - space,
                      child: MinerItem.rent(
                        miner,
                        onTapRent: onTapRent,
                      ),
                    ),
                  )
                  .toList()
              : [],
    );
  }
}
