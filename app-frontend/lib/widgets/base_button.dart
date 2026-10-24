import 'package:flutter/material.dart';

class BaseButton extends StatelessWidget {
  final Widget? child;
  final String? text;
  final EdgeInsetsGeometry? padding;
  final double? width;
  final BorderRadius? borderRadius;
  final bool? loading;
  final String? tag;
  final Function()? onPressed;

  const BaseButton.small({
    super.key,
    this.child,
    this.onPressed,
    this.text,
    this.width,
    this.loading,
    this.tag,
    this.borderRadius = const BorderRadius.all(Radius.circular(50)),
    this.padding = const EdgeInsets.symmetric(
      vertical: 4,
      horizontal: 12,
    ),
  });

  const BaseButton({
    super.key,
    this.child,
    this.onPressed,
    this.loading,
    this.text,
    this.tag,
    this.borderRadius = const BorderRadius.all(Radius.circular(50)),
  })  : width = double.infinity,
        padding = const EdgeInsets.symmetric(vertical: 14);

  bool get canPress => onPressed != null && loading != true;

  Color get bgColor {
    if (!canPress) {
      return const Color(0xFF353535);
    }

    if ([
      "group-list-cancel",
      "modal-cancel",
      "modal-confirm",
    ].contains(tag)) {
      return Colors.transparent;
    }

    if (["cancel"].contains(tag)) {
      return const Color(0xFFff3352);
    }

    if (["my-withdrawal"].contains(tag)) {
      return const Color(0xFF382f20);
    }

    if (["my-top-up"].contains(tag)) {
      return const Color(0xFF2a2a2c);
    }

    if (["logout"].contains(tag)) {
      return const Color(0xFF222224);
    }

    return const Color(0xFFd1a55c);
  }

  Color get textColor {
    if (!canPress) {
      return const Color(0xFF6b6b6e);
    }
    if (["group-list-cancel"].contains(tag)) {
      return const Color(0xFFff3352);
    }

    if (["cancel"].contains(tag)) {
      return const Color(0xFFFFFFFF);
    }

    if ([
      "logout",
      "modal-cancel",
      "modal-confirm",
    ].contains(tag)) {
      return const Color(0xFFd1a55c);
    }
    return const Color(0xFF040404);
  }

  Decoration get decoration {
    if (["group-list-cancel"].contains(tag)) {
      return BoxDecoration(
        border: Border.all(
          color: const Color(0xFFff3352),
        ),
        borderRadius: borderRadius,
      );
    }

    return const BoxDecoration();
  }

  @override
  Widget build(BuildContext context) {
    return Material(
      color: bgColor,
      borderRadius: borderRadius,
      child: InkWell(
        borderRadius: borderRadius,
        onTap: canPress ? onPressed : null,
        child: Container(
          padding: padding,
          width: width,
          decoration: decoration,
          child: loading != null && loading!
              ? const Center(
                  child: SizedBox(
                    height: 20,
                    width: 20,
                    child: CircularProgressIndicator(),
                  ),
                )
              : text != null
                  ? Text(
                      text!,
                      style: TextStyle(
                        color: textColor,
                        fontWeight: FontWeight.w600,
                      ),
                      textAlign: TextAlign.center,
                    )
                  : child,
        ),
      ),
    );
  }
}
