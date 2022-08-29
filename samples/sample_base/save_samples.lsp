;Save document to all dwg and dxf formats the folder should be empty
(defun c:save_samples () 
  (command "FILEDIA" 0 "")

  (setq cur_dir (getvar "dwgprefix"))

  (command "SAVEAS" "2018" (strcat cur_dir "sample_AC1032.dwg"))
  (command "SAVEAS" "2013" (strcat cur_dir "sample_AC1027.dwg"))
  (command "SAVEAS" "2010" (strcat cur_dir "sample_AC1024.dwg"))
  (command "SAVEAS" "2007" (strcat cur_dir "sample_AC1021.dwg"))
  (command "SAVEAS" "2004" (strcat cur_dir "sample_AC1018.dwg"))
  (command "SAVEAS" "2000" (strcat cur_dir "sample_AC1015.dwg"))
  (command "SAVEAS" "LT98" (strcat cur_dir "sample_AC1014.dwg"))

  (command "SAVEAS" "DXF" "V" "2018" "16" (strcat cur_dir "sample_AC1032_ascii.dxf"))
  (command "SAVEAS" "DXF" "V" "2013" "16" (strcat cur_dir "sample_AC1027_ascii.dxf"))
  (command "SAVEAS" "DXF" "V" "2010" "16" (strcat cur_dir "sample_AC1024_ascii.dxf"))
  (command "SAVEAS" "DXF" "V" "2007" "16" (strcat cur_dir "sample_AC1021_ascii.dxf"))
  (command "SAVEAS" "DXF" "V" "2004" "16" (strcat cur_dir "sample_AC1018_ascii.dxf"))
  (command "SAVEAS" "DXF" "V" "2000" "16" (strcat cur_dir "sample_AC1015_ascii.dxf"))
  (command "SAVEAS" "DXF" "V" "R12" "16" (strcat cur_dir "sample_R12_ascii.dxf"))

  (command "SAVEAS" "DXF" "V" "2018" "B" (strcat cur_dir "sample_AC1032_binary.dxf"))
  (command "SAVEAS" "DXF" "V" "2013" "B" (strcat cur_dir "sample_AC1027_binary.dxf"))
  (command "SAVEAS" "DXF" "V" "2010" "B" (strcat cur_dir "sample_AC1024_binary.dxf"))
  (command "SAVEAS" "DXF" "V" "2007" "B" (strcat cur_dir "sample_AC1021_binary.dxf"))
  (command "SAVEAS" "DXF" "V" "2004" "B" (strcat cur_dir "sample_AC1018_binary.dxf"))
  (command "SAVEAS" "DXF" "V" "2000" "B" (strcat cur_dir "sample_AC1015_binary.dxf"))
  (command "SAVEAS" "DXF" "V" "R12" "B" (strcat cur_dir "sample_R12_binary.dxf"))

  (command "FILEDIA" 1 "")
  (princ)
)