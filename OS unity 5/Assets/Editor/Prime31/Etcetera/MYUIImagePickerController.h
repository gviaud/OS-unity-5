//
//  MyUIImagePickerController.h
//  Unity-iPhone
//
//  Created by Eric Benacek on 05/01/12.
//  Copyright (c) 2012 Pointcube. All rights reserved.
//

#import <UIKit/UIKit.h>


@interface MyUIImagePickerController : UIImagePickerController

- (void)takePicture;

- (void)imagePickerController:(UIImagePickerController *)picker 
        didFinishPickingImage:(UIImage *)image
                  editingInfo:(NSDictionary *)editingInfo;

// camera page (overlay view)
- (void)cancel;


@end
