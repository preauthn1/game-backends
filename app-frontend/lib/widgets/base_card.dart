import 'package:flutter/material.dart';

class BaseCard extends StatelessWidget {
  final EdgeInsetsGeometry? padding;
  final EdgeInsetsGeometry? margin;
  final double? height;
  final double? width;
  final Widget? child;
  final double? borderRadius;
  final Function()? onTap;
  const BaseCard({
    super.key,
    this.margin,
    this.padding,
    this.height,
    this.width,
    this.child,
    this.onTap,
    this.borderRadius = 10,
  });

  @override
  Widget build(BuildContext context) {
    final radius = BorderRadius.circular(borderRadius!);
    return Container(
      margin: margin,
      decoration: BoxDecoration(
        borderRadius: radius,
        border: Border.all(
          color: const Color(0xFF252525),
          width: 1,
        ),
        color: const Color(0xFF18171C),
      ),
      child: Material(
        color: Colors.transparent,
        child: InkWell(
          borderRadius: radius,
          onTap: onTap,
          child: ClipRRect(
            borderRadius: radius,
            child: Container(
              padding: padding,
              height: height,
              width: width,
              child: child,
            ),
          ),
        ),
      ),
    );
  }
}
