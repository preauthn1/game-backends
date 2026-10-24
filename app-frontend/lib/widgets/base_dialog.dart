import 'package:flutter/material.dart';

import 'base_button.dart';

class BaseDialog extends StatelessWidget {
  final String? titleText;
  final String? contentText;
  final Widget? content;
  final String? message;
  final bool showCancel;
  final Function()? onConfirm;

  const BaseDialog({
    super.key,
    this.titleText,
    this.content,
    this.message,
    this.onConfirm,
    this.showCancel = false,
    this.contentText = "Confirm",
  });

  @override
  Widget build(BuildContext context) {
    return Dialog(
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(20.0),
      ),
      backgroundColor: const Color(0xFF1b1b1b),
      child: Container(
        constraints: const BoxConstraints(
          maxWidth: 700,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            if (titleText != null)
              Padding(
                padding: const EdgeInsets.only(top: 20.0),
                child: Text(
                  titleText!,
                  style: const TextStyle(
                    fontSize: 20,
                    fontWeight: FontWeight.w600,
                  ),
                ),
              ),
            Padding(
              padding: const EdgeInsets.all(40),
              child: message != null
                  ? Text(message!, textAlign: TextAlign.center)
                  : content ?? const SizedBox.shrink(),
            ),
            Container(
              decoration: const BoxDecoration(
                border: Border(
                  top: BorderSide(
                    color: Color(0xFF545458),
                    width: 1,
                  ),
                ),
              ),
              width: double.infinity,
              child: Row(
                children: [
                  if (showCancel)
                    Expanded(
                      child: BaseButton(
                        text: "Cancel",
                        tag: "modal-cancel",
                        borderRadius: const BorderRadius.only(
                          bottomLeft: Radius.circular(20),
                        ),
                        onPressed: () {
                          Navigator.of(context).pop();
                        },
                      ),
                    ),
                  if (showCancel)
                    Container(
                      width: 1,
                      height: 48,
                      color: const Color(0xFF545458),
                    ),
                  Expanded(
                    child: BaseButton(
                      text: contentText,
                      tag: "modal-confirm",
                      borderRadius: BorderRadius.only(
                        bottomLeft: Radius.circular(showCancel ? 0 : 20),
                        bottomRight: const Radius.circular(20),
                      ),
                      onPressed: onConfirm,
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}
