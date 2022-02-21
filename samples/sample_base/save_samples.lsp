;Save document to all dwg and dxf formats the folder should be empty
(defun c:save_samples () 
  (command "FILEDIA" 0 "")

  (setq cur_dir (getvar "dwgprefix"))

  (command "SAVEAS" "2018" (strcat cur_dir "sample_2018.dwg"))
  (command "SAVEAS" "2013" (strcat cur_dir "sample_2013.dwg"))
  (command "SAVEAS" "2010" (strcat cur_dir "sample_2010.dwg"))
  (command "SAVEAS" "2007" (strcat cur_dir "sample_2007.dwg"))
  (command "SAVEAS" "2004" (strcat cur_dir "sample_2004.dwg"))
  (command "SAVEAS" "2000" (strcat cur_dir "sample_2000.dwg"))
  (command "SAVEAS" "LT98" (strcat cur_dir "sample_R14.dwg"))

  (command "SAVEAS" "DXF" "V" "2018" "16" (strcat cur_dir "sample_2018_ascii.dxf"))
  (command "SAVEAS" "DXF" "V" "2013" "16" (strcat cur_dir "sample_2013_ascii.dxf"))
  (command "SAVEAS" "DXF" "V" "2010" "16" (strcat cur_dir "sample_2010_ascii.dxf"))
  (command "SAVEAS" "DXF" "V" "2007" "16" (strcat cur_dir "sample_2007_ascii.dxf"))
  (command "SAVEAS" "DXF" "V" "2004" "16" (strcat cur_dir "sample_2004_ascii.dxf"))
  (command "SAVEAS" "DXF" "V" "2000" "16" (strcat cur_dir "sample_2000_ascii.dxf"))
  (command "SAVEAS" "DXF" "V" "R12" "16" (strcat cur_dir "sample_R12_ascii.dxf"))

  (command "SAVEAS" "DXF" "V" "2018" "B" (strcat cur_dir "sample_2018_binary.dxf"))
  (command "SAVEAS" "DXF" "V" "2013" "B" (strcat cur_dir "sample_2013_binary.dxf"))
  (command "SAVEAS" "DXF" "V" "2010" "B" (strcat cur_dir "sample_2010_binary.dxf"))
  (command "SAVEAS" "DXF" "V" "2007" "B" (strcat cur_dir "sample_2007_binary.dxf"))
  (command "SAVEAS" "DXF" "V" "2004" "B" (strcat cur_dir "sample_2004_binary.dxf"))
  (command "SAVEAS" "DXF" "V" "2000" "B" (strcat cur_dir "sample_2000_binary.dxf"))
  (command "SAVEAS" "DXF" "V" "R12" "B" (strcat cur_dir "sample_R12_binary.dxf"))

  (command "FILEDIA" 1 "")
  (princ)
)